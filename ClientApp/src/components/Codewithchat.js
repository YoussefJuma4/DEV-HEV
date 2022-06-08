import React from "react";
import CodeEditor from "./CodeEditor";
import { useState } from 'react';
import Newchatroom from "./newchatroom";
import RoomSelector from './RoomSelector'

const Codewithchat = () => {
    
    return (<div className="codePage">
        <Newchatroom />
        <div class="verticalLine"></div>
        <RoomSelector />
    </div>
        
    )
}

export default Codewithchat;