import { Drawer as HeroDrawer } from "@heroui/react";
import { X } from "lucide-react";
import type { ReactNode } from "react";

interface DrawerProps {
	open: boolean;
	title: string;
	children: ReactNode;
	footer: ReactNode;
	onClose: () => void;
}

/**
 * A slide-over whose form lives in the body and whose submit button lives in the footer. The two
 * are siblings here, so callers tie them together with `<form id>` in the body and
 * `<Button type="submit" form={id}>` in the footer - `FormDrawer` is the variant that wraps both
 * in the form itself.
 */
export function Drawer({ open, title, children, footer, onClose }: DrawerProps) {
	return (
		<DrawerFrame open={open} title={title} onClose={onClose}>
			<HeroDrawer.Body>{children}</HeroDrawer.Body>
			<HeroDrawer.Footer>{footer}</HeroDrawer.Footer>
		</DrawerFrame>
	);
}

interface DrawerFrameProps {
	open: boolean;
	title: string;
	onClose: () => void;
	/** Wraps the header and the children, so `FormDrawer` can put its `<form>` around both. */
	wrap?: (content: ReactNode) => ReactNode;
	children: ReactNode;
}

/**
 * The shell both drawers share. Escape and the cross close it through `onClose`; the backdrop
 * does not, as it never did - and with `isDismissable` off HeroUI also drops drag-to-dismiss,
 * which would otherwise throw a half-filled form away on a careless swipe.
 */
export function DrawerFrame({ open, title, onClose, wrap, children }: DrawerFrameProps) {
	const content = (
		<>
			<HeroDrawer.Header>
				<HeroDrawer.Heading>{title}</HeroDrawer.Heading>
				<HeroDrawer.CloseTrigger aria-label="Close" className="action-button">
					<X size={17} />
				</HeroDrawer.CloseTrigger>
			</HeroDrawer.Header>
			{children}
		</>
	);

	return (
		<HeroDrawer.Backdrop
			isOpen={open}
			onOpenChange={(isOpen) => {
				if (!isOpen) onClose();
			}}
			isDismissable={false}
		>
			<HeroDrawer.Content placement="right">
				<HeroDrawer.Dialog>{wrap ? wrap(content) : content}</HeroDrawer.Dialog>
			</HeroDrawer.Content>
		</HeroDrawer.Backdrop>
	);
}
