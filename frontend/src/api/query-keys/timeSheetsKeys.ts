/*
 * Everything hangs under `all` and every write invalidates that one key. A month is not an island:
 * approving somebody's sheet changes their row in the monitoring list, the approval queue it was
 * picked from and the settlement list it lands in, and all three read different endpoints.
 */
export const timeSheetsKeys = {
	all: ["time-sheets"] as const,

	mine: (year: number, month: number) => [...timeSheetsKeys.all, "mine", year, month] as const,

	team: (year: number, month: number) => [...timeSheetsKeys.all, "team", year, month] as const,

	settlement: (year: number, month: number) =>
		[...timeSheetsKeys.all, "settlement", year, month] as const,

	sheet: (userId: string, year: number, month: number) =>
		[...timeSheetsKeys.all, "sheet", userId, year, month] as const,
};
