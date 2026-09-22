import { Avatar } from "./Avatar";

interface ItemMarkProps {
	name?: string | null;
}

/** Initials in a list row. The picture-carrying version is <see cref="Avatar" /> with a `src`. */
export function ItemMark({ name }: ItemMarkProps) {
	return <Avatar className="data-avatar" name={name} />;
}
