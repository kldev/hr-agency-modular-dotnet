import { useForm } from "@tanstack/react-form";
import { Trash } from "lucide-react";
import { useCallback, useState } from "react";
import type { Tag } from "#/api/models";
import { Button, FieldError, TagsPicker } from "#/components/ui";
import { ApiError } from "#/components/ui/ApiError";
import { addIfNotExists, removeFromArray } from "#/utlis";

export interface TagFormResult {
	ids: string[];
}

interface TagFormProps {
	onSubmit: (value: TagFormResult) => void;
	error?: Error | null;
	isSubmitting?: boolean;
}

const emptyResult: TagFormResult = { ids: [] };

export function TagForm({ onSubmit, error, isSubmitting }: TagFormProps) {
	const [tags, setTags] = useState<Tag[]>([]);
	const [input, setInput] = useState<string>("");

	const form = useForm({
		defaultValues: emptyResult,
		onSubmit: async ({ value }) => {
			onSubmit(value);
		},
	});

	const removeTag = useCallback((tag: Tag) => {
		setTags((currentTags) =>
			removeFromArray(currentTags, (currentTag) => currentTag.id === tag.id),
		);
	}, []);

	return (
		<form
			id="tag-form"
			className="drawer-form"
			onSubmit={(event) => {
				event.preventDefault();
				void form.handleSubmit();
			}}
		>
			<form.Field
				name="ids"
				validators={{
					onChange: ({ value }) => {
						if (!value || value.length === 0) {
							return "At leas one tag must be selected";
						}

						return undefined;
					},
				}}
			>
				{(field) => (
					<div className="form-field">
						<label className="form-label" htmlFor={field.name}>
							Tags
						</label>

						<TagsPicker
							disabled={isSubmitting}
							value={null}
							inputValue={input}
							closeOnSelect={true}
							onInputChange={(v) => setInput(v)}
							onChange={(value, item) => {
								if (!item) {
									return;
								}

								const result = addIfNotExists(tags, item, (a, b) => a.id === b.id);

								if (!result.added) {
									return;
								}

								setTags(result.items);
								field.handleChange([...field.state.value, value as string]);

								setInput("");
							}}
						/>

						<FieldError errors={field.state.meta.errors} />
						<div className="tags-list p-2 mt-2n">
							<table className="table">
								{tags.map((z) => (
									<tr key={z.id}>
										<td>{z.name}</td>
										<td align="right">
											<Button
												type="button"
												onClick={() => removeTag(z)}
												variant="ghost"
												icon={<Trash size={14} />}
											></Button>
										</td>
									</tr>
								))}
							</table>
						</div>
					</div>
				)}
			</form.Field>
			<ApiError error={error as unknown as Parameters<typeof ApiError>[0]["error"]} />
		</form>
	);
}
