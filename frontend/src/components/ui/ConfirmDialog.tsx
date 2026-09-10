import { AlertTriangle } from "lucide-react";
import { Button } from "./Button";
import { Dialog } from "./Dialog";

interface ConfirmDialogProps {
	open: boolean;
	title: string;
	description: string;
	confirmLabel?: string;
	loading?: boolean;
	danger?: boolean;
	onConfirm: () => void;
	onClose: () => void;
}

export function ConfirmDialog({
	open,
	title,
	description,
	confirmLabel = "Confirm",
	loading = false,
	danger = true,
	onConfirm,
	onClose,
}: ConfirmDialogProps) {
	return (
		<Dialog
			maxWidth="md"
			open={open}
			title={title}
			onClose={onClose}
			footer={
				<Button variant={danger ? "danger" : "primary"} onClick={onConfirm} loading={loading}>
					{confirmLabel}
				</Button>
			}
		>
			<div className="confirm-dialog">
				<div className="confirm-dialog-icon">
					<AlertTriangle size={20} />
				</div>

				<div>
					<p className="confirm-dialog-description">{description}</p>
				</div>
			</div>
		</Dialog>
	);
}
