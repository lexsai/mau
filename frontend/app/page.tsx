'use server'

import Form from "next/form";
import { cookies } from "next/headers";
import MainMenu from "./main";
import Link from "next/link";

export default async function Home() {
  const cookieStore = await cookies();
  const hasName = cookieStore.has('name');

  async function getName(formData: FormData) {
    'use server'
    const cookieStore = await cookies();
    cookieStore.set("name", formData.get("name") as string);
  }

  if (hasName) {
    return (
      <MainMenu />
    )
  } else {
    return (
      <div className="bg-red-700 flex h-screen">
        <div className="absolute top-[45%] left-[50%] transform translate-x-[-50%] translate-y-[-50%] text-center flex flex-col">
          <Link href="/" className="text-white text-5xl py-2">mau</Link>
          <Form action={getName}>
            <div className="text-white text-1xl">Enter a name:</div>
            <input name="name" className="text-black" />
            <button type="submit" className="text-black bg-red-200 hover:bg-red-400 px-5 my-2">Submit</button>
          </Form>
        </div>
      </div>
    )
  }
}
