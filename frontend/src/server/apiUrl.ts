/// Where the .NET API lives. Its own module rather than a constant on the proxy route, because the
/// proxy now needs the session helpers and the session helpers need this - the route would be a
/// cycle.
export const API_URL = process.env.API_URL ?? "http://localhost:5000";
