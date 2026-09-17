import clsx from "clsx";

const optionClass = [
	"flex cursor-pointer items-start gap-3 rounded-[var(--radius-md)]",
	"border bg-(--color-surface) p-3 transition",
	"hover:border-(--color-primary) hover:bg-(--color-surface-hover)",
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
	onChange,
}: ChoiceGroupProps<T>) {
	return (
		<fieldset className={className} disabled={disabled}>
			{label ? (
				<legend className="mb-3 text-sm font-medium text-(--color-text-secondary)">{label}</legend>
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
						<label
							key={option}
							className={clsx(
								optionClass,
								value === option
									? "border-(--color-primary) bg-(--color-primary-soft)"
									: "border-(--color-border)",
								disabled && "cursor-not-allowed opacity-60",
							)}
						>
							<input
								type="radio"
								name={name}
								value={option}
								checked={value === option}
								disabled={disabled}
								onChange={() => onChange(option)}
								className="mt-0.5 h-4 w-4 accent-(--color-primary)"
							/>

							<span>
								<span className="block text-sm font-medium">{options[option]}</span>

								{description ? (
									<span className="mt-0.5 block text-xs leading-5 text-(--color-text-muted)">
										{description}
									</span>
								) : null}
							</span>
						</label>
					);
				})}
			</div>
		</fieldset>
	);
}
