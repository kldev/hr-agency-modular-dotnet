import { Pencil, Plus, Trash2 } from "lucide-react";
import type { ContactRole, ProjectProjection } from "@/api/models";
import { Button, DetailOverviewHeader } from "@/components/ui";
import {
	agencySideContactRoles,
	clientSideContactRoles,
	contactRoleDescriptions,
	contactRoles,
} from "../../../types";

interface ProjectContactsSectionProps {
	project: ProjectProjection;
	onAssign: (role: ContactRole) => void;
	onRemove: (role: ContactRole) => void;
}

function RoleRow({
	project,
	role,
	onAssign,
	onRemove,
}: {
	project: ProjectProjection;
	role: ContactRole;
	onAssign: (role: ContactRole) => void;
	onRemove: (role: ContactRole) => void;
}) {
	const assigned = project.contacts.find((contact) => contact.role === role);

	return (
		// biome-ignore lint/a11y/useSemanticElements: one role and its holder, read-only - not a form group.
		<div className="project-role-row" role="group" aria-label={contactRoles[role]}>
			<div>
				<div className="project-role-label">{contactRoles[role]}</div>

				{assigned ? (
					<div className="project-role-person">
						{assigned.person.fullname ?? `${assigned.person.firstName} ${assigned.person.lastName}`}
						{assigned.person.jobTitle ? ` · ${assigned.person.jobTitle}` : ""}
						{assigned.person.email ? ` · ${assigned.person.email}` : ""}
					</div>
				) : (
					<div className="project-role-empty">Not assigned</div>
				)}

				<div className="project-role-description">{contactRoleDescriptions[role]}</div>
			</div>

			<div className="flex gap-2">
				<Button
					variant="ghost"
					icon={assigned ? <Pencil size={15} /> : <Plus size={15} />}
					onClick={() => onAssign(role)}
				>
					{assigned ? "Change" : "Assign"}
				</Button>

				{assigned ? (
					<Button variant="ghost" icon={<Trash2 size={15} />} onClick={() => onRemove(role)}>
						Remove
					</Button>
				) : null}
			</div>
		</div>
	);
}

export function ProjectContactsSection({
	project,
	onAssign,
	onRemove,
}: ProjectContactsSectionProps) {
	return (
		<div className="data-overview">
			<DetailOverviewHeader
				title="Contacts"
				description="One person per role. Assigning somebody to a role replaces whoever held it."
			/>

			<div className="project-section-list">
				{clientSideContactRoles.map((role) => (
					<RoleRow
						key={role}
						project={project}
						role={role}
						onAssign={onAssign}
						onRemove={onRemove}
					/>
				))}

				<div className="project-role-group-title">On our side</div>

				{agencySideContactRoles.map((role) => (
					<RoleRow
						key={role}
						project={project}
						role={role}
						onAssign={onAssign}
						onRemove={onRemove}
					/>
				))}
			</div>
		</div>
	);
}
