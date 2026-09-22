import axios from "axios";
import { Trash2 } from "lucide-react";
import { useState } from "react";
import type { MyProfileResponse } from "#/api/models";
import { Avatar, Button } from "#/components/ui";
import { ApiError } from "#/components/ui/ApiError";
import { FileDropzone } from "#/components/ui/FileDropzone";
import { formatFileSize } from "#/utlis";
import {
	ALLOWED_AVATAR_TYPES,
	AVATAR_ACCEPT,
	avatarUrl,
	MAX_AVATAR_SIZE_BYTES,
	useRemoveOwnAvatar,
	useUploadOwnAvatar,
} from "../pages/hooks";

/** The storage being down is not a broken form, and it should not read like one. */
function storageUnavailable(error: unknown) {
	return axios.isAxiosError(error) && error.response?.status === 503;
}

interface AvatarUploaderProps {
	profile: MyProfileResponse;
}

export function AvatarUploader({ profile }: AvatarUploaderProps) {
	const [fileError, setFileError] = useState<string | null>(null);

	const { mutation: upload, progress } = useUploadOwnAvatar({
		onSuccess: () => {
			upload.reset();
		},
	});

	const remove = useRemoveOwnAvatar({
		onSuccess: () => {
			remove.reset();
		},
	});

	const isBusy = upload.isPending || remove.isPending;
	const source = avatarUrl(profile.avatarFileId);

	/*
	 * Checked here as well as on the backend. Not as a gate - the client is not one - but because
	 * finding out that a picture is too large after uploading it is a waste of somebody's time.
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

		upload.mutate(picked);
	};

	return (
		<div className="profile-avatar">
			<Avatar className="profile-avatar-preview" name={profile.user.fullName} src={source} />

			<div className="profile-avatar-controls">
				{/*
				 * The picture is sent the moment it is picked, so the zone never holds a "chosen"
				 * file of its own - the preview on the left is the confirmation.
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

				{profile.avatarFileId ? (
					<div className="profile-avatar-buttons">
						<Button
							variant="ghost"
							icon={<Trash2 size={15} />}
							disabled={isBusy}
							onClick={() => remove.mutate()}
						>
							Remove picture
						</Button>
					</div>
				) : null}

				{storageUnavailable(upload.error) || storageUnavailable(remove.error) ? (
					<div className="form-error" role="alert">
						Picture storage is unavailable, so the file could not be stored. Everything else on this
						page works; only the picture needs the file service running.
					</div>
				) : (
					<>
						<ApiError error={upload.error as unknown as Parameters<typeof ApiError>[0]["error"]} />
						<ApiError error={remove.error as unknown as Parameters<typeof ApiError>[0]["error"]} />
					</>
				)}
			</div>
		</div>
	);
}
