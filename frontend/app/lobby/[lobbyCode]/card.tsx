'use client'

import Image from "next/image";
import { MouseEventHandler } from "react";

export default function Card(
  { value, onClick } : {
    value: string, 
    onClick: MouseEventHandler
  }
) {
  return (
      <Image 
        alt={value} 
        src={`/fronts/${value}.svg`}
        className="cursor-pointer"
        onClick={onClick} 
        width={100} height={100} />
  )
}