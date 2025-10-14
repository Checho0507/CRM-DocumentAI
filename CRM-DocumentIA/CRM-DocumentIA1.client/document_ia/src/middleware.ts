export { default } from "next-auth/middleware";

export const config = { matcher: ["/analytics/:path*",
    "/documents/:path*",
    "/insights/:path*",
    "/dashboard/:path*"] };
