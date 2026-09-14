import { redirect } from "@tanstack/router-core";
import axios, { type AxiosError, type AxiosRequestConfig } from "axios";
import type { BadRequestDetails } from "./models";

const api = axios.create({
	baseURL: "/",
	paramsSerializer: {
		indexes: null,
	},
});

api.interceptors.response.use(
	(response) => response,
	(error) => {
		if (axios.isAxiosError(error) && error.response?.status === 401) {
			throw redirect({ to: "/login" });
		}

		if (axios.isAxiosError(error) && error.response?.status === 400) {
			return Promise.reject(error.response.data as BadRequestDetails);
		}

		return Promise.reject(error);
	},
);

export const customInstance = <T>(
	config: AxiosRequestConfig,
	options?: AxiosRequestConfig,
): Promise<T> => {
	return api({
		...config,
		...options,
	}).then(({ data }) => data);
};

export type ErrorType<Error> = AxiosError<Error>;
export type BodyType<BodyData> = BodyData;
