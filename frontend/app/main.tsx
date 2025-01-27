'use client'

import Link from "next/link"
import { redirect } from "next/navigation";

interface LobbyStartResponse {
  name: string
}

export default function MainMenu() {
  async function createGame() {
    const res = await fetch('http://localhost:5000/lobby/start', { method: 'POST' });
    const obj = await res.json() as LobbyStartResponse;
    redirect(`/lobby/${obj.name}`);
  }

  return (
    <div className="bg-red-700 flex h-screen">
      <div className="absolute top-[45%] left-[50%] transform translate-x-[-50%] translate-y-[-50%] text-center flex flex-col">
        <Link href="/" className="text-white text-5xl py-2">mau</Link>
        <button onClick={createGame} className="text-black bg-red-200 hover:bg-red-400 px-5 py-1 my-2">create lobby</button>
        <Link href="/lobby" className="text-black bg-red-200 hover:bg-red-400 px-5 py-1">join lobby</Link>
      </div>
    </div>
  )
}
