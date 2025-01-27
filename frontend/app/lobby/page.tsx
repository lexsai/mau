'use client'

import { redirect } from 'next/navigation';
import Form from 'next/form'
import Link from 'next/link';

export default function JoinLobby() {
  function joinLobby(formData: FormData) {
    let joinedLobby = formData.get("joinedLobby") as string;
    const match = joinedLobby.match(/\/lobby\/([0-9a-z]{8})$/);
    
    let code = match === null ? joinedLobby : match[1];
    
    if (/^[0-9a-z]{8}$/.test(code)) {
      redirect(`/lobby/${code}`);
    }
  }

  return (
    <div className="bg-red-700 flex h-screen">
      <div className="absolute top-[45%] left-[50%] transform translate-x-[-50%] translate-y-[-50%] text-center flex flex-col">
        <Link href="/" className="text-white text-5xl py-2">mau</Link>
        <div className="text-white text-1xl">Enter a lobby code or URL:</div>
        <Form action={joinLobby}>
          <input name="joinedLobby" className="text-black" />
          <button type="submit" className="text-black bg-red-200 hover:bg-red-400 px-5 my-2">Join</button>
        </Form>
        <Link href="/" className="text-white bg-red-700 hover:bg-red-800">Back</Link>
      </div>
    </div>
  )
}
