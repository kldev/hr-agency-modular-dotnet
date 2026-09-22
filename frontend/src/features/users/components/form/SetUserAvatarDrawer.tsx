import axios from "axios";
import { Trash2 } from "lucide-react";
import { forwardRef, useImperativeHandle, useState } from "react";
import { toast } from "sonner";
import type { UserProjection } from "#/api/models";
import { Avatar, Button } from "#/components/ui";
import { ApiError } from "#/components/ui/ApiError";
import { Drawer } from "#/components/ui/Drawer";
import { FileDropzone } from "#/components/ui/FileDropzone";
import { formatFileSize } from "#/utlis";
import {
	ALLOWED_AVATAR_TYPES,
	AVATAR_ACCEPT,
	MAX_AVATAR_SIZE_BYTES,
} from "../../../profile/pages/hooks";
import { userAvatarUrl } from "../../avatar";
import { useRemoveUserAvatar, useUploadUserAvatar, useUserAvatars } from "../../pages/hooks";
import type { SetUserAvatarFormCommand } from "./UserFormCommand";

interface SetUserAvatarDrawerProps {
	onSuccess: () => void;
}

/** The storage being down is not a broken form, and it should not read like one. */
function storageUnavailable(error: unknown) {
	return axios.isAxiosError(error) && error.response?.status === 503;
}

const DrawerContent: React.FC<{
	user: UserProjection;
	onSuccess: () => void;
	handleClose: () => void;
}> = ({ user, onSuccess, handleClose }) => {
	const [fileError, setFileError] = useState<string | null>(null);
	const { avatarOf } = useUserAvatars();

	const { mutation: upload, progress } = useUploadUserAvatar({
		onSuccess: () => {
			upload.reset();
			toast.success("Picture updated");
			onSuccess();
		},
	});

	const remove = useRemoveUserAvatar({
		onSuccess: () => {
			remove.reset();
			toast.success("Picture removed");
			onSuccess();
		},
	});

	const isBusy = upload.isPending || remove.isPending;
	const avatarFileId = avatarOf(user.id);

	/*
	 * The same two rules the backend enforces, checked again here. Not as a gate - the client is
	 * never one - but so a picture that was never going to be accepted fails while the file dialog
	 * is still fresh in mind.
	 */
	const pickFile = (picked: File | null) => {
		setFileError(null);

		if (!picked) return;

		if (picked.size > MAX_AVATAR_SIZE_BYTES) {
			setFileError(
				`The picture is ${formatFileSize(picked.size)}; the limit is ${formatFileSize(
					MAX_AVATAR_SIZE_BYTES,
				)}.`,
			);
			return;
		}

		if (picked.type && !ALLOWED_AVATAR_TYPES.includes(picked.type)) {
			setFileError("That file type is not accepted. PNG and JPEG pictures are.");
			return;
		}

		upload.mutate({ userId: user.id, file: picked });
	};

	return (
		<Drawer
			open={true}
			title={`Picture of ${user.fullName ?? user.email}`}
			onClose={handleClose}
			footer={
				<Button variant="secondary" onClick={handleClose}>
					Done
				</Button>
			}
		>
			<div className="profile-avatar">
				<Avatar
					className="profile-avatar-preview"
					name={user.fullName}
					src={userAvatarUrl(user.id, avatarFileId)}
				/>

				<div className="profile-avatar-controls">
					{/*
					 * Sent the moment a file is picked, so the zone never holds a "chosen" file of its
					 * own - the preview on the left is the confirmation. There is nothing to submit,
					 * which is why this is a plain drawer rather than a form one.
					 */}
					<FileDropzone
						label=""
						accept={AVATAR_ACCEPT}
						hint={`PNG or JPEG, up to ${formatFileSize(
							MAX_AVATAR_SIZE_BYTES,
						)}. The picture is not cropped.`}
						error={fileError}
						progress={upload.isPending ? progress : null}
						disabled={isBusy}
						compact
						onSelect={pickFile}
					/>

					{avatarFileId ? (
						<div className="profile-avatar-buttons">
							<Button
								variant="ghost"
								icon={<Trash2 size={15} />}
								disabled={isBusy}
								onClick={() => remove.mutate(user.id)}
							>
								Remove picture
							</Button>
						</div>
					) : null}

					{storageUnavailable(upload.error) || storageUnavailable(remove.error) ? (
						<div className="form-error" role="alert">
							Picture storage is unavailable, so the file could not be stored. Everything else about
							this person works; only the picture needs the file service running.
						</div>
					) : (
						<>
							<ApiError
								error={upload.error as unknown as Parameters<typeof ApiError>[0]["error"]}
							/>
							<ApiError
								error={remove.error as unknown as Parameters<typeof ApiError>[0]["error"]}
							/>
						</>
					)}
				</div>
			</div>
		</Drawer>
	);
};

const SetUserAvatarDrawer = forwardRef<SetUserAvatarFormCommand, SetUserAvatarDrawerProps>(
	({ onSuccess }, ref) => {
		const [user, setUser] = useState<UserProjection | null>(null);

		useImperativeHandle(
			ref,
			() => ({
				setAvatar: (next: UserProjection) => {
					setUser(next);
				},
			}),
			[],
		);

		if (!user) return null;

		return <DrawerContent user={user} onSuccess={onSuccess} handleClose={() => setUser(null)} />;
	},
);

SetUserAvatarDrawer.displayName = "SetUserAvatarDrawer";

export default SetUserAvatarDrawer;
