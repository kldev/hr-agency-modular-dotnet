import { FieldError, Label, Radio, RadioGroup } from "@heroui/react";
import clsx from "clsx";
import type { ReactNode } from "react";

/*
 * Each option is a card. Tailwind utilities sit in a later layer than HeroUI's components, so these
 * classes win over `.radio__content`'s own inline row.
 */
const optionClass = [
	"flex w-full cursor-pointer items-start gap-3 rounded-[var(--radius-md)]",
	"border border-(--color-border) bg-(--color-surface) p-3 font-normal text-(--color-text) transition",
	"hover:border-(--color-primary) hover:bg-(--color-surface-hover)",
	"data-[selected=true]:border-(--color-primary) data-[selected=true]:bg-(--color-primary-soft)",
].join(" ");

export type ChoiceGroupProps<T extends string> = {
	label?: string;
	name: string;
	value: T | null;
	options: Record<T, string>;
	descriptions?: Partial<Record<T, string>>;
	columns?: 1 | 2 | 3;
	disabled?: boolean;
	className?: string;
	/** Marks the group invalid and shows the message under it, linked for assistive tech. */
	errorMessage?: ReactNode;
	onChange: (value: T) => void;
};

export function ChoiceGroup<T extends string>({
	label,
	name,
	value,
	options,
	descriptions,
	columns = 3,
	disabled,
	className,
	errorMessage,
	onChange,
}: ChoiceGroupProps<T>) {
	return (
		<RadioGroup
			className={className}
			name={name}
			aria-label={label ? undefined : name}
			value={value ?? null}
			isDisabled={disabled}
			isInvalid={Boolean(errorMessage)}
			// the form's schema is the authority; native validation would add a second message
			validationBehavior="aria"
			onChange={(next) => onChange(next as T)}
		>
			{label ? (
				<Label className="mb-3 text-sm font-medium text-(--color-text-secondary)">{label}</Label>
			) : null}

			<div
				className={clsx(
					"grid gap-3",
					columns === 1 && "md:grid-cols-1",
					columns === 2 && "md:grid-cols-2",
					columns === 3 && "md:grid-cols-3",
				)}
			>
				{(Object.keys(options) as T[]).map((option) => {
					const description = descriptions?.[option];

					return (
						<Radio key={option} value={option} className="mt-0 data-[disabled=true]:opacity-60">
							<Radio.Content className={optionClass}>
								<Radio.Control className="mt-0.5">
									<Radio.Indicator />
								</Radio.Control>

								<span>
									<span className="block text-sm font-medium">{options[option]}</span>

									{description ? (
										<span className="mt-0.5 block text-xs leading-5 text-(--color-text-muted)">
											{description}
										</span>
									) : null}
								</span>
							</Radio.Content>
						</Radio>
					);
				})}
			</div>

			{errorMessage ? (
				<FieldError className="form-field-error mt-2 font-medium">{errorMessage}</FieldError>
			) : null}
		</RadioGroup>
	);
}
