import { Modal } from "@heroui/react";
import clsx from "clsx";
import { X } from "lucide-react";

type Props = {
	open: boolean;
	title: string;
	children: React.ReactNode;
	footer?: React.ReactNode;
	onClose: () => void;
	maxWidth?: "sm" | "md" | "lg" | "wide";
};

/**
 * Every way out - Escape, the cross - goes through `onClose`, never straight to a closed state:
 * a wizard host passes `requestClose` from `useUnsavedChangesGuard` here, and that is what asks
 * before a dirty form disappears. The backdrop does not dismiss, as it never did: one stray click
 * beside a seventeen-field wizard is not a decision to throw it away.
 *
 * Sizes live in `styles/dialog.css` (`dialog--<size>`): `wide` is the wizard's 80vw x 80vh,
 * which takes the whole screen on a phone.
 */
export function Dialog({ open, title, children, footer, onClose, maxWidth = "md" }: Props) {
	return (
		<Modal.Backdrop
			isOpen={open}
			onOpenChange={(isOpen) => {
				if (!isOpen) onClose();
			}}
			isDismissable={false}
		>
			<Modal.Container
				placement="center"
				scroll="inside"
				className={clsx(maxWidth === "wide" && "dialog-container--wide")}
			>
				<Modal.Dialog className={`dialog--${maxWidth}`}>
					<Modal.Header>
						<Modal.Heading>{title}</Modal.Heading>
						<Modal.CloseTrigger aria-label="Close dialog">
							<X size={17} />
						</Modal.CloseTrigger>
					</Modal.Header>

					<Modal.Body>{children}</Modal.Body>

					{footer && <Modal.Footer>{footer}</Modal.Footer>}
				</Modal.Dialog>
			</Modal.Container>
		</Modal.Backdrop>
	);
}
