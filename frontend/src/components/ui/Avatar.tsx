import { Avatar as HeroAvatar } from "@heroui/react";
import clsx from "clsx";

interface AvatarProps {
	/** Used for the initials shown when there is no picture. */
	name?: string | null;
	/** Where to fetch the picture from, or null when the person has none. */
	src?: string | null;
	/** The frame: `data-avatar` for a list row, `profile-avatar-preview` for the profile page. */
	className?: string;
}

function initials(name: string | null | undefined) {
	if (!name) return "";

	return name
		.split(" ")
		.slice(0, 2)
		.map((part) => part[0])
		.join("")
		.toUpperCase();
}

/**
 * A person, as a picture if there is one and as their initials if there is not. The frame - size,
 * border, whether it is round - comes from the caller's class, because a 32px square in a table and
 * a 96px circle on the profile page are the same idea drawn two ways.
 *
 * HeroUI shows the fallback while the picture loads and when it fails, so the file service being
 * down or a reference that outlived its file still leaves a name rather than a broken image.
 */
export function Avatar({ name, src, className }: AvatarProps) {
	return (
		<HeroAvatar className={clsx("panel-avatar", className)}>
			{src ? <HeroAvatar.Image src={src} alt="" /> : null}
			<HeroAvatar.Fallback className="avatar-initials">{initials(name)}</HeroAvatar.Fallback>
		</HeroAvatar>
	);
}
