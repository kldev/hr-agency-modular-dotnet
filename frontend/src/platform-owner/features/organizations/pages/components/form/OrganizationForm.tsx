import { useForm } from "@tanstack/react-form";
import { RefreshCcw } from "lucide-react";
import { useState } from "react";
import type { OrganizationRequest } from "@/api/models";
import { ArrayField, Button, FieldError, Input } from "@/components/ui";
import { ApiError } from "@/components/ui/ApiError";

interface OrganizationFormProps {
	initialValue: OrganizationRequest;
	onSubmit: (value: OrganizationRequest) => void;
	error?: Error | null;
	isSubmitting?: boolean;
}

function createSlug(value: string): string {
	return value
		.normalize("NFD")
		.replace(/[\u0300-\u036f]/g, "")
		.toLowerCase()
		.replace(/[^a-z0-9]+/g, "-")
		.replace(/^-+|-+$/g, "")
		.slice(0, 20)
		.replace(/-+$/, "");
}

export const emptyCreateOrganization: OrganizationRequest = {
	emailDomains: [],
	name: "",
	slug: "",
};

export function OrganizationForm({
	initialValue,
	onSubmit,
	error,
	isSubmitting = false,
}: OrganizationFormProps) {
	const [name, setName] = useState<string>("");
	const form = useForm({
		defaultValues: initialValue,

		onSubmit: async ({ value }) => {
			console.log(`Submit: 1 ${JSON.stringify(value)}`);
			onSubmit(value);
		},
	});

	return (
		<form
			id="organization-form"
			className="drawer-form"
			onSubmit={(event) => {
				event.preventDefault();
				void form.handleSubmit();
			}}
		>
			<form.Field
				name="name"
				validators={{
					onChange: ({ value }) => {
						if (!value.trim()) {
							return "Name is required";
						}

						if (value.length < 3) {
							return "Name should be at least 3 characters long";
						}

						if (value.length > 250) {
							return "Name cannot exceed 250 characters.";
						}

						return undefined;
					},
				}}
			>
				{(field) => (
					<div className="form-field">
						<label className="form-label" htmlFor={field.name}>
							Name
						</label>

						<Input
							id={field.name}
							name={field.name}
							value={field.state.value}
							disabled={isSubmitting}
							onBlur={field.handleBlur}
							onChange={(event) => {
								field.handleChange(event.target.value);
								setName(event.target.value);
							}}
						/>

						<FieldError errors={field.state.meta.errors} />
					</div>
				)}
			</form.Field>

			<form.Field
				name="slug"
				validators={{
					onChange: ({ value }) => {
						if (!value.trim()) {
							return "Slug is required";
						}

						if (value.length < 3) {
							return "Slug should be at least 3 characters long";
						}

						if (value.length > 100) {
							return "Slug cannot exceed 100 characters.";
						}

						return undefined;
					},
				}}
			>
				{(field) => (
					<div className="form-field">
						<label className="form-label" htmlFor={field.name}>
							Slug
						</label>
						<div className="form-input-action">
							<Input
								id={field.name}
								name={field.name}
								value={field.state.value}
								disabled={isSubmitting}
								onBlur={field.handleBlur}
								onChange={(event) => field.handleChange(event.target.value)}
							/>

							<Button
								variant="ghost"
								disabled={name.length < 3}
								icon={<RefreshCcw size={16} />}
								onClick={() => {
									const slug = createSlug(field.form.getFieldValue("name"));
									field.handleChange(slug);
								}}
							></Button>
						</div>

						<FieldError errors={field.state.meta.errors} />
					</div>
				)}
			</form.Field>

			<form.Field
				name="emailDomains"
				validators={{
					onChange: ({ value }) => {
						if (value.length === 0) {
							return "At least one domain is required";
						}

						if (value.every((z) => z.trim().length <= 3)) {
							return "At least one domain is required. With at least 3 characters long";
						}

						return undefined;
					},
				}}
			>
				{(field) => (
					<div className="form-field">
						<label className="form-label" htmlFor={field.name}>
							Email domain ex. <strong>company.com</strong>
						</label>

						<ArrayField
							values={field.state.value}
							label=""
							onChange={(v) => field.handleChange(v)}
						/>

						<FieldError errors={field.state.meta.errors} />
					</div>
				)}
			</form.Field>

			<ApiError error={error as unknown as Parameters<typeof ApiError>[0]["error"]} />
		</form>
	);
}
