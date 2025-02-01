import type { Config } from "tailwindcss";

export default {
  content: [
    "./pages/**/*.{js,ts,jsx,tsx,mdx}",
    "./components/**/*.{js,ts,jsx,tsx,mdx}",
    "./app/**/*.{js,ts,jsx,tsx,mdx}",
  ],
  theme: {
    extend: {
      colors: {
        background: "var(--background)",
        foreground: "var(--foreground)",
      },
      keyframes: {
        highlight: {
          '0%': {
            background: '#78ff78',
          },
          '100%': {
            background: 'none',
          },
        }
      },
      animation: {
        highlight: 'highlight 1s',
      }
    },
  },
  plugins: [],
} satisfies Config;
