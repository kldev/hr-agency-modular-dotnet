import { useState } from "react";
import type { TeamRole } from "@/api/models";
import { EnumSelectFilter, FieldError, RepeatableField, UsersPicker } from "@/components/ui";
import { defaultTeamRole, teamRoles } from "../../types";

export type TeamMemberDraft = {
	/**
	 * Identifies the row, not the person. Each row keeps its own search text, so keying by array
	 * index would leave the previous row's name on screen once a row above it is removed and the
	 * rows below shift up - the picker only writes a resolved label into an input that is empty.
	 */
	key: string;
	userId: string;
	role: TeamRole;
};

let nextRowKey = 0;

export const createMember = (): TeamMemberDraft => ({
	key: `member-${nextRowKey++}`,
	userId: "",
	role: defaultTeamRole,
});

interface TeamMembersFieldProps {
	members: TeamMemberDraft[];
	onChange: (members: TeamMemberDraft[]) => void;
	isSubmitting?: boolean;
	errors: Array<unknown>;
}

/**
 * The founding roster of a team. The domain refuses a team without members, so the last row cannot
 * be removed here either - the button goes disabled rather than letting the request bounce off a
 * 400 the user never asked for.
 */
export function TeamMembersField({
	members,
	onChange,
	isSubmitting,
	errors,
}: TeamMembersFieldProps) {
	const rows = members.length > 0 ? members : [createMember()];

	const update = (index: number, patch: Partial<TeamMemberDraft>) =>
		onChange(rows.map((row, i) => (i === index ? { ...row, ...patch } : row)));

	return (
		<div className="form-field">
			<RepeatableField<TeamMemberDraft>
				label="Members"
				description="Who is on this team, and what they do on it."
				items={rows}
				minItems={1}
				addLabel="Add member"
				disabled={isSubmitting}
				canAdd={rows.every((row) => row.userId.length > 0)}
				getRowKey={(row) => row.key}
				onAdd={() => onChange([...rows, createMember()])}
				onRemove={(index) => onChange(rows.filter((_, i) => i !== index))}
				renderRow={(row, index) => (
					<TeamMemberRow
						row={row}
						index={index}
						isSubmitting={isSubmitting}
						onChange={(patch) => update(index, patch)}
					/>
				)}
			/>

			<FieldError errors={errors} />
		</div>
	);
}

interface TeamMemberRowProps {
	row: TeamMemberDraft;
	index: number;
	isSubmitting?: boolean;
	onChange: (patch: Partial<TeamMemberDraft>) => void;
}

function TeamMemberRow({ row, index, isSubmitting, onChange }: TeamMemberRowProps) {
	// The picker shows a search box, so the query it holds belongs to the row, not to the form value.
	const [input, setInput] = useState("");

	return (
		<div className="flex flex-col gap-2 sm:flex-row sm:items-start">
			<div className="min-w-0 flex-1">
				<UsersPicker
					placeholder={`Person ${index + 1}`}
					disabled={isSubmitting}
					value={row.userId}
					inputValue={input}
					onChange={(id) => onChange({ userId: id ?? "" })}
					onInputChange={setInput}
				/>
			</div>

			<div className="sm:w-44">
				<EnumSelectFilter
					hideAll
					aria-label={`Team role ${index + 1}`}
					value={row.role}
					options={teamRoles}
					onChange={(value) => {
						if (value) {
							onChange({ role: value });
						}
					}}
				/>
			</div>
		</div>
	);
}
