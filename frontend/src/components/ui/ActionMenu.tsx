import { MoreVertical } from "lucide-react";
import { type ComponentType, useCallback, useState } from "react";
import { Dropdown, DropdownDivider, DropdownItem } from "@/components/ui";

export interface ActionMenuItem {
	label: string;
	action: () => void;
	icon?: ComponentType<{ size?: number }>;
	dividerAfter?: boolean;
	disabled?: boolean;
	/** Shown on hover; the place to say why a disabled action is disabled. */
	hint?: string;
}

interface ActionMenuProps {
	actions: ActionMenuItem[];
	ariaLabel?: string;
	title?: string;
}

export function ActionMenu({
	actions,
	ariaLabel = "More actions",
	title = "More actions",
}: ActionMenuProps) {
	const [open, setOpen] = useState(false);

	// Stable, because the dropdown re-subscribes its document listeners whenever this changes.
	const close = useCallback(() => setOpen(false), []);

	return (
		<div className="action-menu">
			<button
				type="button"
				className="action-button action-button-trigger"
				aria-label={ariaLabel}
				title={title}
				aria-expanded={open}
				onClick={() => {
					setOpen((prev) => !prev);
				}}
			>
				<MoreVertical size={20} />
			</button>

			{open ? (
				<Dropdown placement="right" onClose={close}>
					{actions.map((item, index) => {
						const Icon = item.icon;

						return (
							<div key={`drop-item-${index}`}>
								<DropdownItem
									key={`drop-item-${index}`}
									disabled={item.disabled}
									title={item.hint}
									onClick={() => {
										item.action();
										setOpen(false);
									}}
								>
									{Icon && <Icon size={15} />}
									{item.label}
								</DropdownItem>

								{item.dividerAfter && <DropdownDivider />}
							</div>
						);
					})}
				</Dropdown>
			) : null}
		</div>
	);
}
