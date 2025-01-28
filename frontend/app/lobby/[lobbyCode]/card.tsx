'use client'

import Image from "next/image";
import { MouseEventHandler } from "react";

export default function Card(
  { value, onClick, highlight } : {
    value: string, 
    onClick: MouseEventHandler,
    highlight: boolean
  }
) {
  return (
      <Image 
        alt={value} 
        src={`/fronts/${value}.svg`}
        className={"cursor-pointer" + (highlight ? " p-2" : "")}
        onClick={onClick} 
        width={100} height={100} />
  )
}