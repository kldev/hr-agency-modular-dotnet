export function generatePassword(length = 10): string {
	const lower = "abcdefghijklmnopqrstuvwxyz";
	const upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
	const digits = "0123456789";
	const all = lower + upper + digits;

	const getRandomChar = (chars: string) => chars[Math.floor(Math.random() * chars.length)];

	const required = [getRandomChar(lower), getRandomChar(upper), getRandomChar(digits)];

	for (let i = required.length; i < length; i++) {
		required.push(getRandomChar(all));
	}

	return required.sort(() => Math.random() - 0.5).join("");
}
