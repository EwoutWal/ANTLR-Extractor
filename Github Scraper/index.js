require.config({
    paths: {
        'axios': 'https://cdn.jsdelivr.net/npm/axios/dist/axios.min'
    }
});

require(["axios"], () => {
    const btn = document.getElementById("getButton");
    btn.addEventListener("click", getCodeFiles);
    
    async function getCodeFiles(){
        for (var page = 31; page < 35; page += 1){
            const url = `https://api.github.com/search/code?q=grammar\+extension:g4\&page=${page}`;
            await axios.get(url, { headers: { Authorization: 'token 1a8cee937d5b64222dbe5c66d21d6784e4d6d1aa' } }).then(res => setResult(res.data, page)).catch();
        }
    }
    
    async function setResult(res, page_number){
        const resultDiv = document.getElementById("results");
        resultString = '';
        console.log(`Page ${page_number}`)
        res.items.forEach(element => {
            if (!element.repository.fork) {
                resultString += `${element.git_url}\n`;
            }
            else{
                console.log("Fork, skipping")
            }
        });
        resultDiv.innerText += resultString;
    }
});
