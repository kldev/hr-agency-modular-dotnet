import { forwardRef, useCallback, useImperativeHandle, useState } from "react";
import type { TagRequestList } from "#/api/models";
import { DetailOverviewHeader, SaveChangesButton } from "#/components/ui";
import { Drawer } from "#/components/ui/Drawer";
import type { AddTagCommand, TagTarget } from "./TagCommand";
import { TagForm, type TagFormResult } from "./TagForm";
import { useTag } from "./useTag";

interface AddTagDrawerProps {
	onSuccess: () => void;
}

const AddTagsDrawer = forwardRef<AddTagCommand, AddTagDrawerProps>(({ onSuccess }, ref) => {
	const [isOpen, setIsOpen] = useState(false);
	const [tagTarget, setTagTarget] = useState<TagTarget | null>(null);
	const [id, setId] = useState<string | null>(null);
	const [display, setDisplay] = useState<string>("");

	const { tag, isPending, error, waiting } = useTag({
		onSuccess: () => {
			handleClose();
			onSuccess();
		},
	});

	useImperativeHandle(
		ref,
		() => ({
			addTag: (id: string, display: string, target: TagTarget) => {
				console.log(`Add tag ${id}, ${target}`);
				setId(id);
				setDisplay(display);
				setTagTarget(target);
				setIsOpen(true);
			},
		}),
		[],
	);

	const handleSave = useCallback(
		(data: TagFormResult) => {
			if (!id || !tagTarget) {
				return;
			}

			const request: TagRequestList = {
				tagIds: data.ids,
			};
			tag(tagTarget, id, request);
		},
		[id, tag, tagTarget],
	);

	const handleClose = () => {
		setId(null);
		setDisplay("");
		setTagTarget(null);
		setIsOpen(false);
	};

	const title = tagTarget === "application" ? "Tag job application" : "Tag candidate";

	return (
		<Drawer
			open={isOpen}
			title={title}
			onClose={handleClose}
			footer={<SaveChangesButton form="tag-form" isPending={isPending} wait={waiting} />}
		>
			{display.length ? (
				<DetailOverviewHeader
					className=" mb-4"
					title={display}
					description=""
				></DetailOverviewHeader>
			) : null}
			<TagForm onSubmit={handleSave} error={error} isSubmitting={isPending} />
		</Drawer>
	);
});

AddTagsDrawer.displayName = "AddTagsDrawer";

export default AddTagsDrawer;
