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
    <div className="bg-red-500 flex h-screen">
      <div className="absolute top-[45%] left-[50%] transform translate-x-[-50%] translate-y-[-50%] text-center flex flex-col">
        <div className="text-white text-5xl py-2">mau</div>
        <button onClick={createGame} className="text-white bg-red-700 hover:bg-red-800 px-5 py-1 my-2">create lobby</button>
        <Link href="/lobby" className="text-white bg-red-700 hover:bg-red-800 px-5 py-1">join lobby</Link>
      </div>
    </div>
  )
}
