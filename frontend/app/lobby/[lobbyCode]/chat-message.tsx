'use client'

import Form from "next/form";
import { useRef } from "react";
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
    <>
      <div className={message.sender == "The Dealer" ? "text-red-900 font-bold" : "text-blue-900"}>{message.sender} says:</div>
      <div className="text-black ml-5">{content}</div>
    </>
  )
}

export default function ChatBox({
   playerName, messages, sendCallback 
} : { 
  playerName: string,
  messages: ChatMessage[], 
  sendCallback: Function
}) {
  const historyRef = useRef<HTMLDivElement>(null);

  function updateScroll() {
    if (historyRef.current == null) {
      return;
    }
    const history = historyRef.current;

    let shouldScroll = ((history.scrollHeight - history.offsetHeight) == history.scrollTop)
    shouldScroll = shouldScroll || history.offsetHeight == 0;

    if (shouldScroll) {
      history.scrollTop = history.scrollHeight;
    }
  }
  setInterval(updateScroll, 100);

  function sendMessage(formData: FormData) {
    sendCallback(formData.get("sendContent"));
  }

  return <div className=" bg-white w-[600px] text-black border-2 border-black flex flex-col text-left">
    <div ref={historyRef} className="h-96 shrink-0 overflow-auto">
      {messages.map((message, index) => <MessageElement message={message} 
                                                        playerName={playerName} 
                                                        key={message.sender + message.timeSent + message.content} />)}
    </div>
    <Form action={sendMessage} className="flex w-full border-t-2 border-black">
      <input name="sendContent" autoComplete="off" className="text-black grow" />
      <button type="submit" className="text-black border-l-2 border-black bg-red-400 hover:bg-red-400 px-5">Send</button>
    </Form>
  </div>
}