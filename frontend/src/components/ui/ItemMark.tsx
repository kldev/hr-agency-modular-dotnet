interface ItemMarkProps {
	name?: string | null
}

export function ItemMark({ name }: ItemMarkProps) {
	const initials = !name || name.length === 0 ? "" :
		name
			.split(" ")
			.slice(0, 2)
			.map((part) => part[0])
			.join("")
			.toUpperCase()

	return <span className="data-avatar">{initials}</span>;
}