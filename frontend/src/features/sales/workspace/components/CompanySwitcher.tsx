import { useState } from "react";
import { CompaniesPicker } from "#/components/ui/pickers";

/** A company search that hands over the pick and forgets it - the page owns which one is shown. */
export function CompanySwitcher({ onPick }: { onPick: (companyId: string) => void }) {
	const [input, setInput] = useState("");

	return (
		<CompaniesPicker
			id="workspace-company"
			label="Company"
			placeholder="Search companies..."
			value=""
			inputValue={input}
			onInputChange={setInput}
			onChange={(id) => {
				if (id) {
					setInput("");
					onPick(id);
				}
			}}
		/>
	);
}
