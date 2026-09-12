import { useForm } from "@tanstack/react-form";
import { KeyRound } from "lucide-react";
import { toast } from "sonner";
import { type CreateUserRequest, OrganizationRoleApi } from "@/api/models";
import { Button, EnumSelectFilter, FieldError, Input } from "@/components/ui";
import { ApiError } from "@/components/ui/ApiError";
import { copyToClipboard, generatePassword } from "@/utlis";
import { organizationRoles } from "../../types";

interface UserFormProps {
	initialValue: CreateUserRequest;
	onSubmit: (value: CreateUserRequest) => void;
	error?: Error | null;
	isSubmitting?: boolean;
}

export const emptyCreateUser: CreateUserRequest = {
	email: "",
	firstName: "",
	lastName: "",
	role: OrganizationRoleApi.Recruiter,
	password: "",
};

export function UserForm({ initialValue, onSubmit, error, isSubmitting = false }: UserFormProps) {
	const form = useForm({
		defaultValues: initialValue,

		onSubmit: async ({ value }) => {
			onSubmit(value);
		},
	});

	return (
		<form
			id="user-form"
			className="drawer-form"
			onSubmit={(event) => {
				event.preventDefault();
				void form.handleSubmit();
			}}
		>
			<form.Field
				name="email"
				validators={{
					onChange: ({ value }) => {
						if (!value.trim()) {
							return "Email is required";
						}

						if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value)) {
							return "Enter a valid email address";
						}

						return undefined;
					},
				}}
			>
				{(field) => (
					<div className="form-field">
						<label className="form-label" htmlFor={field.name}>
							Email
						</label>

						<Input
							id={field.name}
							name={field.name}
							type="email"
							autoComplete="email"
							value={field.state.value}
							disabled={isSubmitting}
							onBlur={field.handleBlur}
							onChange={(event) => field.handleChange(event.target.value)}
						/>

						<FieldError errors={field.state.meta.errors} />
					</div>
				)}
			</form.Field>

			<form.Field
				name="firstName"
				validators={{
					onChange: ({ value }) => (value.trim() ? undefined : "First name is required"),
				}}
			>
				{(field) => (
					<div className="form-field">
						<label className="form-label" htmlFor={field.name}>
							First name
						</label>

						<Input
							id={field.name}
							name={field.name}
							autoComplete="given-name"
							value={field.state.value}
							disabled={isSubmitting}
							onBlur={field.handleBlur}
							onChange={(event) => field.handleChange(event.target.value)}
						/>

						<FieldError errors={field.state.meta.errors} />
					</div>
				)}
			</form.Field>

			<form.Field
				name="lastName"
				validators={{
					onChange: ({ value }) => (value.trim() ? undefined : "Last name is required"),
				}}
			>
				{(field) => (
					<div className="form-field">
						<label className="form-label" htmlFor={field.name}>
							Last name
						</label>

						<Input
							id={field.name}
							name={field.name}
							autoComplete="family-name"
							value={field.state.value}
							disabled={isSubmitting}
							onBlur={field.handleBlur}
							onChange={(event) => field.handleChange(event.target.value)}
						/>

						<FieldError errors={field.state.meta.errors} />
					</div>
				)}
			</form.Field>

			<form.Field name="role">
				{(field) => (
					<div className="form-field">
						<label className="form-label" htmlFor={field.name}>
							Role
						</label>

						<EnumSelectFilter
							hideAll
							value={field.state.value}
							options={organizationRoles}
							onChange={(value) => {
								if (value) {
									field.handleChange(value);
								}
							}}
						/>

						<FieldError errors={field.state.meta.errors} />
					</div>
				)}
			</form.Field>

			<form.Field
				name="password"
				validators={{
					onChange: ({ value }) => {
						if (!value) {
							return "Password is required";
						}

						if (value.length < 3) {
							return "Password must contain at least 3 characters";
						}

						return undefined;
					},
				}}
			>
				{(field) => (
					<div className="form-field">
						<label className="form-label" htmlFor={field.name}>
							Password
						</label>

						<div className="form-input-action">
							<Input
								id={field.name}
								name={field.name}
								type="password"
								autoComplete="new-password"
								value={field.state.value}
								disabled={isSubmitting}
								onBlur={field.handleBlur}
								onChange={(event) => field.handleChange(event.target.value)}
							/>

							<Button
								variant="ghost"
								icon={<KeyRound size={16} />}
								onClick={async () => {
									const password = generatePassword();
									await copyToClipboard(`User password: ${password}`);
									field.handleChange(password);
									toast.info("Password coppied to clipboard");
								}}
							></Button>
						</div>
						<div className="form-hint">The user can reset the password later.</div>

						<FieldError errors={field.state.meta.errors} />
					</div>
				)}
			</form.Field>

			<ApiError error={error as unknown as Parameters<typeof ApiError>[0]["error"]} />
		</form>
	);
}
