downloadFileStream=async(e,a)=>{let o=new Blob([await a.arrayBuffer()]),r=URL.createObjectURL(o),t=document.createElement("a");t.href=r,t.download=e??"",t.click(),t.remove(),URL.revokeObjectURL(r)},triggerFileDownload=(e,a)=>{let o=document.createElement("a");o.href=a,o.download=e??"",o.click(),o.remove()};
//# sourceMappingURL=BlazorControls.js.map
