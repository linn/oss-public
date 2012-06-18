function GetQueryString(){return function(a){if(a=="")return{};var b={};for(var i=0;i<a.length;++i){var p=a[i].split('=');b[p[0]]=decodeURIComponent(p[1].replace(/\+/g," "))}return b}(window.location.hash.substr(1).split('&'))}
            
