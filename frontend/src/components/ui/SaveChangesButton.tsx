import { Loader } from "lucide-react";
import { Button } from "./Button";

type SaveChangesButtonProps = {
	wait: boolean;
	isPending: boolean;
	form: string;
	onClick?: () => void;
};

export function SaveChangesButton({ wait, isPending, form, onClick }: SaveChangesButtonProps) {
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
			{isPending ? "Saving..." : "Save changes"}
		</Button>
	);
}
