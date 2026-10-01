import { ListBox } from "@heroui/react";
import type { ReactNode } from "react";
import { jobLanguages } from "../types/languages";
import { SelectField } from "./Select";

type LanguageSelectProps = {
	id?: string;
	name?: string;
	label?: string;
	"aria-label"?: string;
	/** A language code, or "" / null for none. */
	value: string | null;
	onChange: (code: string) => void;
	isDisabled?: boolean;
	errorMessage?: ReactNode;
	onBlur?: () => void;
	className?: string;
};

/** The ten languages the agency publishes in - short enough for a plain select. */
export function LanguageSelect(props: LanguageSelectProps) {
	return (
		<SelectField {...props} placeholder="Select language">
			{Object.entries(jobLanguages).map(([code, name]) => (
				<ListBox.Item key={code} id={code} textValue={`${code} — ${name}`}>
					{code} — {name}
					<ListBox.ItemIndicator />
				</ListBox.Item>
			))}
		</SelectField>
	);
}
