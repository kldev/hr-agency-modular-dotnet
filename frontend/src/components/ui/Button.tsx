import { Button as HeroButton, type ButtonProps as HeroButtonProps, Spinner } from "@heroui/react";
import clsx from "clsx";
import type { ReactNode } from "react";

type Variant = "primary" | "secondary" | "ghost" | "danger" | "back";

/*
 * The panel's names for HeroUI's variants. "secondary" here has always been the outlined button on
 * the surface - HeroUI calls that "outline"; its own "secondary" is a filled neutral button the
 * panel has never had. buttons.css dresses each one in the panel's colours.
 */
const HERO_VARIANT: Record<Variant, NonNullable<HeroButtonProps["variant"]>> = {
	primary: "primary",
	secondary: "outline",
	ghost: "ghost",
	danger: "danger",
	back: "secondary",
};

export type ButtonProps = Omit<HeroButtonProps, "variant" | "children"> & {
	variant?: Variant;
	icon?: ReactNode;
	/** A native tooltip. React Aria does not pass `title` through, so it is put back on the element. */
	title?: string;
	children?: ReactNode;
};

/**
 * HeroUI's Button with the two things every call site here wants: an icon slot, and a spinner in
 * its place while the action is pending.
 */
export function Button({
	variant = "secondary",
	icon,
	title,
	isPending = false,
	children,
	className,
	...props
}: ButtonProps) {
	return (
		<HeroButton
			{...props}
			isPending={isPending}
			variant={HERO_VARIANT[variant]}
			className={clsx(variant === "back" && "button-back", className)}
			render={title ? (domProps) => <button {...domProps} title={title} /> : undefined}
		>
			{isPending ? <Spinner color="current" size="sm" /> : icon}
			{children}
		</HeroButton>
	);
}

type ActionButtonProps = Omit<HeroButtonProps, "children" | "isIconOnly"> & {
	children?: ReactNode;
	title: string;
};

/** The icon-only button of a table row or a card: the label is its tooltip and its accessible name. */
export function ActionButton({ children, title, className, ...props }: ActionButtonProps) {
	return (
		<HeroButton
			{...props}
			isIconOnly
			variant="ghost"
			size="sm"
			aria-label={title}
			className={clsx("action-button", className)}
			render={(domProps) => <button {...domProps} title={title} />}
		>
			{children}
		</HeroButton>
	);
}
