export { default } from "next-auth/middleware";

export const config = { matcher: ["/analyticss/:path*",
    "/documentss/:path*",
    "/insightss/:path*",
    "/dashboards/:path*"] };
