import { useMutation } from "@tanstack/react-query";
import { forwardRef, useCallback, useImperativeHandle, useState } from "react";
import { postJobToChannel } from "@/api/endpoints";
import type { PostToChannelRequest } from "@/api/models";
import { SaveChangesButton } from "@/components/ui";
import { Drawer } from "@/components/ui/Drawer";
import { useProjectionWait } from "@/hooks";
import type { PostToChannelCommand } from "../JobPostsCommand";
import { emptyRequest, PostToChannelForm } from "./PostToChannelForm";

interface PostToChannelDrawerProps {
	onSuccess: () => void;
}

const PostToChannelDrawer = forwardRef<PostToChannelCommand, PostToChannelDrawerProps>(
	({ onSuccess }, ref) => {
		const [isOpen, setIsOpen] = useState(false);
		const [jobPostId, setJobPostId] = useState<string | null>(null);

		const { wait, waiting } = useProjectionWait();

		const mutation = useMutation({
			mutationFn: ({ id, request }: { id: string; request: PostToChannelRequest }) =>
				postJobToChannel(id, request),

			onSuccess: async () => {
				await wait();

				setIsOpen(false);
				setJobPostId(null);

				onSuccess();
			},
		});

		useImperativeHandle(
			ref,
			() => ({
				postToChannel: (id: string) => {
					mutation.reset();
					setJobPostId(id);
					setIsOpen(true);
				},
			}),
			[mutation],
		);

		const handleSave = useCallback(
			(data: PostToChannelRequest) => {
				if (!jobPostId) {
					return;
				}

				mutation.mutate({
					id: jobPostId,
					request: data,
				});
			},
			[jobPostId, mutation],
		);

		const handleClose = useCallback(() => {
			if (mutation.isPending) {
				return;
			}

			mutation.reset();
			setIsOpen(false);
			setJobPostId(null);
		}, [mutation]);

		return (
			<Drawer
				open={isOpen}
				title="Post job to channel"
				onClose={handleClose}
				footer={
					<SaveChangesButton
						form="post-to-channel-form"
						isPending={mutation.isPending}
						wait={waiting}
					/>
				}
			>
				<PostToChannelForm
					initialValue={emptyRequest}
					onSubmit={handleSave}
					error={mutation.error}
				/>
			</Drawer>
		);
	},
);

PostToChannelDrawer.displayName = "PostToChannelDrawer";

export default PostToChannelDrawer;
