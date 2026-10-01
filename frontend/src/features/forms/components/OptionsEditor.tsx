import { Input } from "@heroui/react";
import { ArrowDown, ArrowUp, Plus, Trash2 } from "lucide-react";
import type { ChoiceOption } from "#/api/models";
import { Button } from "#/components/ui";

type OptionsEditorProps = {
	idPrefix: string;
	options: ChoiceOption[];
	disabled?: boolean;
	onChange: (options: ChoiceOption[]) => void;
};

/**
 * The answers a choice offers. The value is what is stored and reported on, the label what people
 * read, so a relabelled answer does not split a report in two - which is why both are asked for.
 */
export function OptionsEditor({ idPrefix, options, disabled, onChange }: OptionsEditorProps) {
	const update = (index: number, patch: Partial<ChoiceOption>) =>
		onChange(options.map((option, i) => (i === index ? { ...option, ...patch } : option)));

	const move = (index: number, by: -1 | 1) => {
		const target = index + by;

		if (target < 0 || target >= options.length) return;

		const copy = [...options];
		[copy[index], copy[target]] = [copy[target], copy[index]];
		onChange(copy);
	};

	return (
		<div className="flex flex-col gap-2">
			<span className="form-label">Options</span>

			{options.map((option, index) => (
				<div key={index} className="flex items-center gap-2">
					<Input
						aria-label={`Label of option ${index + 1}`}
						id={`${idPrefix}-label-${index}`}
						placeholder="Label"
						disabled={disabled}
						value={option.label}
						onChange={(event) => update(index, { label: event.target.value })}
					/>
					<Input
						aria-label={`Value of option ${index + 1}`}
						id={`${idPrefix}-value-${index}`}
						placeholder="Stored value"
						className="max-w-40"
						disabled={disabled}
						value={option.value}
						onChange={(event) => update(index, { value: event.target.value })}
					/>
					<Button
						variant="ghost"
						icon={<ArrowUp size={14} />}
						aria-label="Up"
						isDisabled={disabled}
						onPress={() => move(index, -1)}
					/>
					<Button
						variant="ghost"
						icon={<ArrowDown size={14} />}
						aria-label="Down"
						isDisabled={disabled}
						onPress={() => move(index, 1)}
					/>
					<Button
						variant="ghost"
						icon={<Trash2 size={14} />}
						aria-label="Remove option"
						isDisabled={disabled}
						onPress={() => onChange(options.filter((_, i) => i !== index))}
					/>
				</div>
			))}

			{disabled ? null : (
				<div>
					<Button
						variant="secondary"
						icon={<Plus size={14} />}
						onPress={() =>
							onChange([...options, { value: `option${options.length + 1}`, label: "" }])
						}
					>
						Add option
					</Button>
				</div>
			)}
		</div>
	);
}
