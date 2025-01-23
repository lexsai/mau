'use client'

import { useEffect, useState } from "react"
import { HubConnection, HubConnectionBuilder  } from "@microsoft/signalr";
import { useParams } from "next/navigation";

export default function Game({ playerName } : { playerName: string }) {
  const params = useParams<{ lobbyCode: string }>();
  const lobbyCode = params.lobbyCode;
  const [connection, setConnection] = useState<HubConnection|null>(null);

  useEffect(() => {
    setConnection(
      new HubConnectionBuilder()
        .withUrl("http://localhost:5000/game")
        .build()
    );

    connection?.invoke('JoinLobby', lobbyCode, )
  }, []);

  return (
    <h1>{playerName} is in {lobbyCode}</h1>
  )
}