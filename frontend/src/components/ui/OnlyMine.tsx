import clsx from "clsx";
import { UserRound } from "lucide-react";
import { Button } from "./Button";

type OnlyMineProps = {
	checked: boolean;
	onChange: (value: boolean) => void;
	label?: string;
};

export function OnlyMine({
	onChange,
	checked: onlyMine,
	label = "My opportunities",
}: OnlyMineProps) {
	return (
		<Button
			type="button"
			variant="secondary"
			icon={<UserRound size={15} />}
			aria-label="My opportunities"
			aria-pressed={onlyMine}
			onClick={() => onChange(!onlyMine)}
			className={clsx(
				"shrink-0 whitespace-nowrap",
				onlyMine && [
					"border-(--color-primary)!",
					"bg-(--color-primary-soft)!",
					"text-(--color-primary)!",
					"font-semibold",
					"ring-1",
					"ring-(--color-primary)",
				],
			)}
		>
			{label}
			{onlyMine && (
				<span
					className="ml-1.5 h-1.5 w-1.5  rounded-full bg-(--color-primary)"
					aria-hidden="true"
				/>
			)}
		</Button>
	);
}
