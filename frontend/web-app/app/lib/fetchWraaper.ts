import { getTokenWorkaround } from "../actions/authAction";

const baseUrl = "http://localhost:6001/";

const handleResponse = async (response: Response) => {
  const text = await response.text(); // the response body is not always json, so I use text
  const data = text && JSON.parse(text);

  if (response.ok) {
    return data || response.statusText;
  } else {
    const error = {
      status: response.status,
      message: response.statusText,
    };
    return error;
  }
};

const get = async (url: string) => {
  const requestOptions = {
    method: "GET",
    headers: await getHeaders(),
  };

  const response = await fetch(baseUrl + url, requestOptions);
  return await handleResponse(response);
};

const post = async (url: string, body: {}) => {
  const requestOptions = {
    method: "POST",
    headers: await getHeaders(),
    body: JSON.stringify(body),
  };
  const response = await fetch(baseUrl + url, requestOptions);

  return await handleResponse(response);
};

const put = async (url: string, body: {}) => {
  const requestOptions = {
    method: "PUT",
    headers: await getHeaders(),
    body: JSON.stringify(body),
  };
  const response = await fetch(baseUrl + url, requestOptions);

  return await handleResponse(response);
};

const del = async (url: string) => {
  const requestOptions = {
    method: "DELETE",
    headers: await getHeaders(),
  };
  const response = await fetch(baseUrl + url, requestOptions);

  return await handleResponse(response);
};

const getHeaders = async () => {
  const token = await getTokenWorkaround();
  const headers = { "Content-type": "application/json" } as any;
  if (token) {
    headers.Authorization = "Bearer " + token.assess_token;
  }

  return headers;
};

export const fetchWrapper = {
  get,
  post,
  put,
  del,
};