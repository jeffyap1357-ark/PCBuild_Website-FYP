/**
 * Chatbot: keyword-based help, introduces all webpage features
 */
(function () {
    var wrap = document.getElementById('chatbot-wrap');
    var panel = document.getElementById('chatbot-panel');
    var messages = document.getElementById('chatbot-messages');
    var input = document.getElementById('chatbot-input');
    var sendBtn = document.getElementById('chatbot-send');
    var toggleBtn = document.getElementById('chatbot-toggle');
    var closeBtn = document.getElementById('chatbot-close');

    if (!wrap || !messages || !input) return;

    var isLoggedIn = document.body.classList.contains('chatbot-logged-in') ||
        (document.querySelector('a[href="/Build/Configure"]') !== null);

    var helpContent = {
        full: '<p><strong>PC Build Features:</strong></p>' +
            '<ul>' +
            '<li><a href="/">Home</a> — Quick access, top-rated hardware, quiz links</li>' +
            '<li><a href="/Hardware/List">Hardware</a> — Browse CPU, motherboard, GPU, RAM, storage; view details and prices</li>' +
            '<li><a href="/Learning/Index">Learning</a> — PC building tutorials and guides</li>' +
            '<li><a href="/Community/Index">Community</a> — View shared builds from others</li>' +
            '<li><a href="/Recommend">Recommend</a> — Get recommended builds by budget</li>' +
            (isLoggedIn ? '<li><a href="/Build/Configure">Build</a> — Pick compatible parts, save configs</li>' : '') +
            (isLoggedIn ? '<li><a href="/Build/Index">My Builds</a> — Manage saved builds</li>' : '') +
            (isLoggedIn ? '<li><a href="/Quiz/LevelsView">Quiz</a> — Beginner / Normal / Professional levels</li>' : '') +
            (isLoggedIn ? '<li><a href="/Performance/Index">Performance</a> — Select parts and get a performance score</li>' : '') +
            (!isLoggedIn ? '<li><a href="/Account/Login">Login</a> | <a href="/Account/Register">Register</a> — Log in to use Build, Quiz, Performance</li>' : '') +
            '</ul>',
        hardware: '<p><strong>Hardware</strong></p>' +
            '<p>Browse <a href="/Hardware/List">CPU, motherboard, GPU, RAM, storage</a> and more. View details, prices, specs. Compare items from the detail page.</p>',
        quiz: '<p><strong>Quiz</strong></p>' +
            '<p>Test your PC knowledge at <a href="/Quiz/LevelsView">Quiz</a>. Beginner, Normal, and Professional levels. One question per page with explanations. Login required.</p>',
        performance: '<p><strong>Performance</strong></p>' +
            '<p>At <a href="/Performance/Index">Performance Test</a> select CPU, GPU, etc. The system calculates a score and gives an assessment. Login required.</p>',
        build: '<p><strong>Build</strong></p>' +
            '<p>At <a href="/Build/Configure">Build</a> pick compatible CPU, motherboard, RAM, etc. Compatibility is checked. Save and share to community. Login required.</p>',
        learning: '<p><strong>Learning</strong></p>' +
            '<p>Read <a href="/Learning/Index">PC building tutorials</a> and step-by-step guides.</p>',
        community: '<p><strong>Community</strong></p>' +
            '<p>Browse <a href="/Community/Index">shared builds</a>, rate and comment. Share your build from <a href="/Build/Index">My Builds</a>.</p>',
        recommend: '<p><strong>Recommend</strong></p>' +
            '<p>At <a href="/Recommend">Recommend</a> enter your budget to get suggested hardware. Save or configure further.</p>',
        login: '<p><strong>Login / Register</strong></p>' +
            '<p><a href="/Account/Login">Login</a> | <a href="/Account/Register">Register</a> | <a href="/Account/ResetPassword">Reset Password</a></p>' +
            '<p>Log in to create builds, take quizzes, run performance tests, share to community.</p>',
        compatibility: '<p><strong>PC Compatibility</strong></p>' +
            '<p><b>CPU + Motherboard:</b> Match the socket (e.g. Intel LGA1700, AMD AM5). Pick CPU first, then filter motherboards.</p>' +
            '<p><b>RAM:</b> Match DDR type (DDR4/DDR5) and speed. Check motherboard specs.</p>' +
            '<p><b>Case:</b> Match form factor (ATX, mATX, ITX). Motherboard and case must support the same size.</p>' +
            '<p><b>PSU:</b> Enough wattage for CPU + GPU. Add ~150–200W headroom. Use <a href="/Performance/Index">Performance Test</a> to estimate.</p>',
        cpu: '<p><strong>CPU (Processor)</strong></p>' +
            '<p>Choose CPU first when building. Intel and AMD use different sockets — the motherboard must match. F-suffix Intel CPUs (e.g. i5-10400F) have no integrated graphics; you need a GPU. For gaming, balance CPU and GPU; for productivity, a stronger CPU helps.</p>',
        motherboard: '<p><strong>Motherboard</strong></p>' +
            '<p>Must match CPU socket (Intel LGA or AMD AM). Check form factor (ATX, mATX, ITX) for your case. Supports RAM type (DDR4/DDR5) and speed. More VRMs and features cost more.</p>',
        ram: '<p><strong>RAM (Memory)</strong></p>' +
            '<p>Match DDR type (DDR4 or DDR5) to the motherboard. 16GB is minimum for most builds; 32GB for gaming/editing. Speed (e.g. 3200MHz) should be within board support. Install in dual-channel (slots 2 and 4 usually).</p>',
        gpu: '<p><strong>GPU (Graphics Card)</strong></p>' +
            '<p>Needed for gaming and heavy graphics. Match to monitor resolution (1080p, 1440p, 4K). Check case clearance and PSU wattage. NVIDIA and AMD are the main brands.</p>',
        psu: '<p><strong>Power Supply (PSU)</strong></p>' +
            '<p>Wattage = CPU + GPU + ~150W for the rest. 80+ Bronze minimum; Gold for efficiency. Modular cables help with cable management. Never cheap out — a bad PSU can damage parts.</p>',
        cooling: '<p><strong>Cooling</strong></p>' +
            '<p><b>CPU:</b> Stock cooler is fine for non-K CPUs; aftermarket (air or AIO) for overclocking or quiet builds.</p>' +
            '<p><b>Case:</b> Front intake + rear exhaust. More fans = better airflow. Dust filters help.</p>',
        storage: '<p><strong>Storage</strong></p>' +
            '<p><b>SSD (NVMe):</b> Fast boot and load times. Use as main drive.</p>' +
            '<p><b>HDD:</b> Cheaper for bulk storage (games, media).</p>' +
            '<p>M.2 NVMe needs an M.2 slot on the motherboard. SATA for HDDs and SATA SSDs.</p>',
        assembly: '<p><strong>Assembly Steps</strong></p>' +
            '<p>1. Install CPU, cooler, RAM on motherboard (outside case).</p>' +
            '<p>2. Mount motherboard in case with standoffs.</p>' +
            '<p>3. Install PSU, route cables.</p>' +
            '<p>4. Install storage (M.2, SSD, HDD).</p>' +
            '<p>5. Install GPU in PCIe slot.</p>' +
            '<p>6. Connect power cables (24-pin, CPU, GPU, SATA).</p>' +
            '<p>7. Connect front panel (power, reset, LEDs).</p>' +
            '<p>8. Cable management, close case, connect peripherals.</p>' +
            '<p>Read <a href="/Learning/Index">Learning</a> for detailed guides.</p>',
        default: '<p>Type <strong>help</strong> for features. Ask about PC building: <strong>compatibility</strong>, <strong>cpu</strong>, <strong>motherboard</strong>, <strong>ram</strong>, <strong>gpu</strong>, <strong>psu</strong>, <strong>cooling</strong>, <strong>storage</strong>, <strong>assembly</strong>.</p>'
    };

    var keywordMap = [
        { keys: ['help', 'what', 'features', 'intro'], reply: 'full' },
        { keys: ['hardware', 'compare'], reply: 'hardware' },
        { keys: ['quiz', 'test', 'knowledge'], reply: 'quiz' },
        { keys: ['performance', 'score'], reply: 'performance' },
        { keys: ['build', 'pc build', 'configure'], reply: 'build' },
        { keys: ['learning', 'guide', 'tutorial'], reply: 'learning' },
        { keys: ['community', 'share'], reply: 'community' },
        { keys: ['recommend', 'budget'], reply: 'recommend' },
        { keys: ['login', 'register', 'account', 'sign'], reply: 'login' },
        { keys: ['compatibility', 'compatible', 'socket', 'match', 'fit'], reply: 'compatibility' },
        { keys: ['cpu', 'processor', 'intel', 'amd'], reply: 'cpu' },
        { keys: ['motherboard', 'mobo', 'board'], reply: 'motherboard' },
        { keys: ['ram', 'memory', 'ddr'], reply: 'ram' },
        { keys: ['gpu', 'graphics', 'video card', 'nvidia', 'amd gpu'], reply: 'gpu' },
        { keys: ['psu', 'power supply', 'wattage', 'power'], reply: 'psu' },
        { keys: ['cooling', 'cooler', 'fan', 'thermal', 'aio'], reply: 'cooling' },
        { keys: ['storage', 'ssd', 'hdd', 'nvme', 'm.2'], reply: 'storage' },
        { keys: ['assembly', 'assemble', 'install', 'step', 'build pc', 'put together'], reply: 'assembly' }
    ];

    function addMsg(text, isUser) {
        var div = document.createElement('div');
        div.className = 'chatbot-msg chatbot-msg-' + (isUser ? 'user' : 'bot');
        if (isUser) {
            div.textContent = text;
        } else {
            div.innerHTML = text;
        }
        messages.appendChild(div);
        messages.scrollTop = messages.scrollHeight;
    }

    function getReply(text) {
        var t = (text || '').trim().toLowerCase();
        if (!t) return helpContent.default;
        for (var i = 0; i < keywordMap.length; i++) {
            var item = keywordMap[i];
            for (var j = 0; j < item.keys.length; j++) {
                if (t.indexOf(item.keys[j].toLowerCase()) !== -1) {
                    return helpContent[item.reply] || helpContent.default;
                }
            }
        }
        return helpContent.default;
    }

    function send() {
        var text = input.value.trim();
        if (!text) return;
        addMsg(text, true);
        input.value = '';
        addMsg(getReply(text), false);
    }

    function toggle() {
        wrap.classList.toggle('chatbot-open');
        wrap.setAttribute('aria-hidden', wrap.classList.contains('chatbot-open') ? 'false' : 'true');
        if (wrap.classList.contains('chatbot-open')) {
            input.focus();
        }
    }

    function askQuick(keyword) {
        addMsg(keyword, true);
        addMsg(getReply(keyword), false);
    }

    toggleBtn && toggleBtn.addEventListener('click', toggle);
    closeBtn && closeBtn.addEventListener('click', toggle);
    sendBtn && sendBtn.addEventListener('click', send);
    input && input.addEventListener('keydown', function (e) {
        if (e.key === 'Enter') { e.preventDefault(); send(); }
    });
    document.querySelectorAll('.chatbot-quick-btn').forEach(function (btn) {
        btn.addEventListener('click', function () {
            var kw = btn.getAttribute('data-ask');
            if (kw) askQuick(kw);
        });
    });
})();
