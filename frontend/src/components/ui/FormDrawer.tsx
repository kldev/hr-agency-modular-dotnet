import { Drawer as HeroDrawer } from "@heroui/react";
import clsx from "clsx";
import type { FormHTMLAttributes, ReactNode } from "react";
import { DrawerFrame } from "./Drawer";

type FormDrawerProps = Omit<FormHTMLAttributes<HTMLFormElement>, "children"> & {
	open: boolean;
	title: string;
	children: ReactNode;
	onClose: () => void;
};

type FormDrawerContentProps = {
	children: ReactNode;
	className?: string;
};

type FormDrawerFooterProps = {
	children: ReactNode;
	className?: string;
};

/**
 * A drawer that is a form. The `<form>` wraps the header, `FormDrawer.Content` and
 * `FormDrawer.Footer` alike, so a plain `type="submit"` in the footer submits it without the
 * `form={id}` pairing `Drawer` needs. The dialog role and its label stay on HeroUI's dialog.
 */
function FormDrawer({ open, title, children, onClose, className, ...formProps }: FormDrawerProps) {
	return (
		<DrawerFrame
			open={open}
			title={title}
			onClose={onClose}
			wrap={(content) => (
				<form {...formProps} className={clsx("drawer__form", className)}>
					{content}
				</form>
			)}
		>
			{children}
		</DrawerFrame>
	);
}

function Content({ children, className }: FormDrawerContentProps) {
	return <HeroDrawer.Body className={className}>{children}</HeroDrawer.Body>;
}

function Footer({ children, className }: FormDrawerFooterProps) {
	return <HeroDrawer.Footer className={className}>{children}</HeroDrawer.Footer>;
}

export { FormDrawer };

FormDrawer.Content = Content;
FormDrawer.Footer = Footer;
