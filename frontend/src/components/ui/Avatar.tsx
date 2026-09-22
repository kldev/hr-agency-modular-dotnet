import { useState } from "react";

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
 * The `onError` fallback is a safety net rather than the signal: "has no picture" is `src` being
 * null. It catches the file service being down or a reference that outlived its file, which would
 * otherwise leave a broken image where a name should be.
 */
export function Avatar({ name, src, className }: AvatarProps) {
	const [failed, setFailed] = useState(false);

	return (
		<span className={className ? `avatar ${className}` : "avatar"}>
			{src && !failed ? (
				<img src={src} alt="" className="avatar-image" onError={() => setFailed(true)} />
			) : (
				<span className="avatar-initials">{initials(name)}</span>
			)}
		</span>
	);
}
