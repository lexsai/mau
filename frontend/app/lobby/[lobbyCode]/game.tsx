'use client'

import { useEffect, useState } from "react"
import { HubConnection, HubConnectionBuilder  } from "@microsoft/signalr";
import { useParams } from "next/navigation";
import Link from "next/link";

export default function Game({ playerName } : { playerName: string }) {
  const params = useParams<{ lobbyCode: string }>();
  const lobbyCode = params.lobbyCode;
  const [connection, setConnection] = useState<HubConnection|null>(null);
  const [shareUrl, setShareUrl] = useState<string>('');
  const [lobbyMembers, setLobbyMembers] = useState<string[]>([]);
  const [message, setMessage] = useState<string>('');
  const [isAdmin, setIsAdmin] = useState<boolean>(true);

  useEffect(() => {
    setShareUrl(`${window.location.origin}/lobby/${lobbyCode}`);
 
    const conn = new HubConnectionBuilder()
      .withUrl("http://localhost:5000/game")
      .build()
    
    conn.on('LobbyUsersUpdate', (data) => {
      setLobbyMembers(data);
      console.log(data);
    })

    conn.on('WriteMessage', (data) => {
      setMessage(data);
      console.log(data);
    })


    conn.start().then(() => {
      conn.invoke('JoinLobby', lobbyCode, playerName)
    })

    setConnection(conn);

  }, []);

  function startGame() {

  }

  return (
    <div className="bg-red-700 flex h-screen">
      <div className="absolute top-[30%] left-[50%] transform translate-x-[-50%] translate-y-[-50%] text-center flex flex-col">
        <Link href="/" className="text-white text-5xl py-2">mau</Link>
              
        <div className="italic">You are '{playerName}'. {isAdmin && <div>You have authority to start the game.</div>}</div>
        {isAdmin && <button onClick={startGame} className="text-black bg-red-200 hover:bg-red-400 px-5 my-2 mx-auto w-64">Start Game</button>}
        <br />
        <div>Invite others to the lobby with this link: <Link className="text-yellow-300" href={shareUrl}>{shareUrl}</Link></div>
        <br />

        <div>
          <div className="font-bold">Lobby Members:</div>
          {lobbyMembers.map((name, index) => <div key={index}>{name}</div>)}
        </div>

      </div>
    </div>
  )
}