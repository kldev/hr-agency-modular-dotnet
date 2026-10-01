import { AlertDialog } from "@heroui/react";
import { AlertTriangle, X } from "lucide-react";
import { Button } from "./Button";

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

/**
 * HeroUI's AlertDialog, dressed as the panel's `Dialog` (header with a cross, icon beside the
 * text, one button in the footer). AlertDialog ignores Escape by default; here it closes, as the
 * confirmation always did - and stacked on a wizard, only the confirmation hears it, so "keep
 * editing" is one key away.
 */
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
		<AlertDialog.Backdrop
			isOpen={open}
			onOpenChange={(isOpen) => {
				if (!isOpen) onClose();
			}}
			isDismissable={false}
			isKeyboardDismissDisabled={false}
		>
			<AlertDialog.Container placement="center">
				<AlertDialog.Dialog className="dialog--md">
					<AlertDialog.Header>
						<AlertDialog.Heading>{title}</AlertDialog.Heading>
						<AlertDialog.CloseTrigger aria-label="Close dialog">
							<X size={17} />
						</AlertDialog.CloseTrigger>
					</AlertDialog.Header>

					<AlertDialog.Body>
						<div className="confirm-dialog">
							<AlertDialog.Icon className="confirm-dialog-icon">
								<AlertTriangle size={20} />
							</AlertDialog.Icon>

							<div>
								<p className="confirm-dialog-description">{description}</p>
							</div>
						</div>
					</AlertDialog.Body>

					<AlertDialog.Footer>
						<Button variant={danger ? "danger" : "primary"} onPress={onConfirm} isPending={loading}>
							{confirmLabel}
						</Button>
					</AlertDialog.Footer>
				</AlertDialog.Dialog>
			</AlertDialog.Container>
		</AlertDialog.Backdrop>
	);
}
