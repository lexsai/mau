'use server'

import { cookies } from "next/headers"
import Game from "./game"
import { redirect } from "next/navigation";

export default async function Lobby() {
  const cookieStore = await cookies();
  const playerName = cookieStore.get('name')?.value;

  if (playerName === undefined) {
    await redirect('/');
  } else {
    return (
      <Game playerName={playerName} />
    );
  }
}