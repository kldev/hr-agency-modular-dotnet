import { Button, Dropdown, Label, Separator } from "@heroui/react";
import { MoreVertical } from "lucide-react";
import { type ComponentType, Fragment, type HTMLAttributes, type Key } from "react";

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

/** The row menu of a table or a card: HeroUI's dropdown over a list of actions. */
export function ActionMenu({
	actions,
	ariaLabel = "More actions",
	title = "More actions",
}: ActionMenuProps) {
	// The index is the key: labels are not guaranteed unique, and the list is rebuilt every render.
	const run = (key: Key) => actions[Number(key)]?.action();

	return (
		<Dropdown>
			<Button
				isIconOnly
				variant="ghost"
				size="sm"
				aria-label={ariaLabel}
				className="action-button action-button-trigger"
				render={(domProps) => <button {...domProps} title={title} />}
			>
				<MoreVertical size={20} />
			</Button>

			<Dropdown.Popover placement="bottom end" className="action-menu-popover">
				<Dropdown.Menu aria-label={ariaLabel} onAction={run}>
					{actions.map((item, index) => {
						const Icon = item.icon;

						return (
							<Fragment key={index}>
								<Dropdown.Item
									id={String(index)}
									textValue={item.label}
									isDisabled={item.disabled}
									render={
										item.hint
											? // An action is never a link, so the item is always the <div> React Aria renders.
												(domProps) => (
													<div
														{...(domProps as HTMLAttributes<HTMLDivElement>)}
														title={item.hint}
													/>
												)
											: undefined
									}
								>
									{Icon ? <Icon size={15} /> : null}
									<Label>{item.label}</Label>
								</Dropdown.Item>

								{item.dividerAfter ? <Separator /> : null}
							</Fragment>
						);
					})}
				</Dropdown.Menu>
			</Dropdown.Popover>
		</Dropdown>
	);
}
