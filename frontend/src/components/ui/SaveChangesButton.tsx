import { Loader } from "lucide-react";
import { useFormContext } from "#/forms";
import { Button } from "./Button";

type SaveChangesButtonProps = {
	wait: boolean;
	isPending: boolean;
	form: string;
	onClick?: () => void;
	label?: string;
	labelPending?: string;
	disabled?: boolean;
};

export function SaveChangesButton({
	wait,
	isPending,
	form,
	onClick,
	label,
	labelPending,
}: SaveChangesButtonProps) {
	return (
		<Button
			variant="primary"
			type="submit"
			icon={!wait ? null : <Loader size={15} />}
			loading={wait}
			disabled={isPending}
			form={form}
			onClick={onClick}
		>
			{isPending ? (labelPending ?? "Saving...") : (label ?? "Save changes")}
		</Button>
	);
}

type FormSaveChangesButtonProps = {
	wait: boolean;
	isPending: boolean;
};

export function FormSaveChangesButton({ wait, isPending }: FormSaveChangesButtonProps) {
	const formContext = useFormContext();
	return (
		<formContext.Subscribe
			selector={(state) => ({
				isSubmitting: state.isSubmitting,
				isValid: state.isValid,
			})}
		>
			{({ isSubmitting, isValid }) => {
				const isDisabled = isSubmitting || !isValid || isPending;
				return (
					<Button
						variant="primary"
						type="submit"
						icon={!wait ? null : <Loader size={15} />}
						loading={wait}
						disabled={isDisabled}
					>
						Save changes
					</Button>
				);
			}}
		</formContext.Subscribe>
	);
}
