import { useState } from "react";
import axios from "axios";
import "./App.css";

function App() {
  const [url, setUrl] = useState("");
  const [shortUrl, setShortUrl] = useState("");
  const [loading, setLoading] = useState(false);
  const [copied, setCopied] = useState(false);

  const handleSubmit = async () => {
    if (!url) return;

    try {
      setLoading(true);
      setCopied(false);

      const res = await axios.post("http://localhost:8080/api/shorten", {
        url
      });

      setShortUrl(res.data.shortUrl);
    } catch {
      alert("Ошибка запроса");
    } finally {
      setLoading(false);
    }
  };

  const handleCopy = async () => {
    if (!shortUrl) return;

    await navigator.clipboard.writeText(shortUrl);
    setCopied(true);

    setTimeout(() => setCopied(false), 2000);
  };

  return (
    <div className="app">
      <div className="card">

        <h1>Краткий URL - это удобно</h1>

        <div className="input-group">
          <input
            type="text"
            placeholder="Вставь ссылку..."
            value={url}
            onChange={(e) => setUrl(e.target.value)}
          />

          <button onClick={handleSubmit}>
            {loading ? "..." : "Сократить"}
          </button>
        </div>

        {shortUrl && (
          <div className="result">
            <p>Твоя ссылка:</p>

            <div className="short-url-block">
              <div className="short-url">{shortUrl}</div>

              <button className="copy-btn" onClick={handleCopy}>
                {copied ? "Скопировано" : "Копировать"}
              </button>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}

export default App;