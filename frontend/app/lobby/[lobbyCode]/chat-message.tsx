'use client'

import { HubConnection } from "@microsoft/signalr";
import Form from "next/form";
import { useEffect, useRef, useState } from "react";
import reactStringReplace from "react-string-replace";

export interface ChatMessage {
  sender: string;
  content: string;
  timeSent: number;
}

function MessageElement({message, playerName}: {message: ChatMessage, playerName: string}) {
  const content = reactStringReplace(
    message.content, 
    playerName, 
    (match, i, key) => <div className="inline underline text-red-900 text-green-900" key={key}>{playerName}(YOU)</div>);
  return (
    <div className="animate-highlight">
      <div className={message.sender == "The Dealer" ? "text-red-900 font-bold" : "text-blue-900"}>{message.sender} says:</div>
      <div className="text-black ml-5">{content}</div>
    </div>
  )
}

let shouldScroll = false;

export default function ChatBox({
   playerName, connection 
} : { 
  playerName: string,
  connection: HubConnection
}) {
  const [chatMessages, setChatMessages] = useState<ChatMessage[]>([]);

  const historyRef = useRef<HTMLDivElement>(null);
  
  useEffect(() => {
    connection.on('ChatMessage', (data) => {
      if (historyRef.current != null) {
        const history = historyRef.current; 
        shouldScroll = (history.scrollTop + history.clientHeight === history.scrollHeight);
      }
      setChatMessages((messages) => [...messages, data]);
    })
  }, []);

  useEffect(() => {
    if (historyRef.current == null) {
      return;
    }

    if (shouldScroll) {
      historyRef.current.scrollTop = historyRef.current.scrollHeight;
    }
  }, [chatMessages])


  function sendMessage(formData: FormData) {
    connection.invoke("sendChat", formData.get("sendContent") as string);
  }

  return <div className=" bg-white w-[600px] text-black border-2 border-black flex flex-col text-left">
    <div ref={historyRef} className="h-96 shrink-0 overflow-auto">
      {chatMessages.map((message, index) => <MessageElement message={message} 
                                                            playerName={playerName} 
                                                            key={message.sender + message.timeSent + message.content} />)}
    </div>
    <Form action={sendMessage} className="flex w-full border-t-2 border-black">
      <input name="sendContent" autoComplete="off" className="text-black grow" />
      <button type="submit" className="text-black border-l-2 border-black bg-red-400 hover:bg-red-400 px-5">Send</button>
    </Form>
  </div>
}