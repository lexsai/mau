'use client'

import { useEffect, useState } from "react"
import { HubConnection, HubConnectionBuilder  } from "@microsoft/signalr";
import { useParams } from "next/navigation";
import Link from "next/link";
import Card from "./card";
import Image from "next/image";
import ChatBox, { ChatMessage } from "./chat-message";

const until = (predFn: Function, timeout: number) => {
  let timeoutElapsed = false;
  setTimeout(() => timeoutElapsed = true, timeout);
  const poll = (done: Function) => (predFn() || timeoutElapsed ? done() : setTimeout(() => poll(done), 500));
  return new Promise(poll);
};

let cardInput = -1;

export default function Game({ playerName } : { playerName: string }) {
  const params = useParams<{ lobbyCode: string }>();
  const lobbyCode = params.lobbyCode;
  const [connection, setConnection] = useState<HubConnection|null>(null);
  const [shareUrl, setShareUrl] = useState<string>('');
  
  const [lobbyMembers, setLobbyMembers] = useState<string[]>([]);
  const [lastTurnPlayer, setLastTurnPlayer] = useState<string>('');
  const [message, setMessage] = useState<string>('');
  const [isAdmin, setIsAdmin] = useState<boolean>(false);
  const [gameStarted, setGameStarted] = useState<boolean>(false);
  const [hand, setHand] = useState<string[]>([]);
  const [lastPlayedCard, setLastPlayedCard] = useState<string>('');
  const [awaitingInput, setAwaitingInput] = useState<boolean>(false);
  const [selectedCard, setSelectedCard] = useState<number>(-1);
 
  useEffect(() => {
    setShareUrl(`${window.location.origin}/lobby/${lobbyCode}`);
 
    const conn = new HubConnectionBuilder()
      .withUrl("http://localhost:5000/game")
      .build()
    
    conn.on('LobbyUsersUpdate', (data) => {
      setLobbyMembers(data);
      console.log(data);
    })

    conn.on('HandUpdate', (data) => {
      setHand(data);
      setSelectedCard(-1);
    })

    conn.on('PlayedCardUpdate', (data) => {
      setLastPlayedCard(data);
    })

    conn.on('TurnUpdate', (data) => {
      setLastTurnPlayer(data);
    })

    conn.on('WriteMessage', (data) => {
      setMessage(data);
      console.log(data);
    })

    conn.on('GameStarted', (data) => {
      setGameStarted(true);

      setHand(data);
    })

    conn.on('NotifyAdmin', () => {
      setIsAdmin(true);
    }) 

    conn.on('RequestCard', async () => {
      setAwaitingInput(true);
      await until(() => cardInput != -1, 10000);
      
      let tmpInput = cardInput;
      cardInput = -1;
      console.log(tmpInput);
      return tmpInput.toString();
    })

    conn.start().then(() => {
      conn.invoke('JoinLobby', lobbyCode, playerName)
      console.log("tried join");
    })

    setConnection(conn);

    return () => {
      conn.stop();
    }
  }, []);

  function startGame() {
    connection?.invoke('StartGame')
  }
  
  function playCard(index: number) {
    if (!awaitingInput) {
      return;
    }

    console.log("clicked", index);
    setSelectedCard(index);
    setAwaitingInput(false);
    cardInput = index;
  }

  if (!gameStarted) {
    return (
      <div className="bg-red-700 flex h-screen">
        <div className="absolute top-[30%] left-[50%] transform translate-x-[-50%] translate-y-[-50%] text-center flex flex-col">
          <Link href="/" className="text-white text-5xl py-2">mau</Link>
                
          <div className="italic">You are '{playerName}'. {isAdmin && <div>You are an admin of this lobby.</div>}</div>
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
  } else {
    return (
      <div className="bg-red-700 flex h-screen">
        <div className="absolute top-[5%] left-[50%] transform translate-x-[-50%] text-center flex">
          <div className="shrink-0">
            <div className="font-bold text-yellow-200">
              Your name is "{playerName}".
            </div>
            <br/>
            <div className="font-bold">Lobby Members:</div>
            {lobbyMembers.map((name, index) => {
              if (name == lastTurnPlayer) {
                return <div className="font-bold text-yellow-100" key={index}>{name} (last turn)</div>
              } else {
                return <div key={index}>{name}</div>;
              }
            })}
          </div>

          <div className="flex text-center flex-col w-[600px] shrink-0">
            {(lastPlayedCard !== '') && <div>
              <div className="font-bold">Last Played:</div>
              <Image className="mx-auto " src={`/fronts/${lastPlayedCard}.svg`} alt={lastPlayedCard} width={100} height={100} />
            </div>}
            <br />
            <div className="font-bold">{message}</div>
            <br />
            <div className="font-bold">Hand:</div>
            <div className="flex flex-wrap items-center justify-center max-w-3xl">
              <Card highlight={selectedCard === -2} 
                    value="pass" onClick={() => playCard(-2)} />
              {hand.map((card, index) => <Card highlight={selectedCard === index} 
                                               value={card} 
                                               onClick={() => playCard(index)} key={index} />)}
            </div>
          </div>
          <div className="mx-10">
            <ChatBox playerName={playerName} connection={connection as HubConnection} />
          </div>
        </div>
      </div>
    )
  }
}