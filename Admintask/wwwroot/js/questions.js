(function () {
  "use strict";

  const modal = document.getElementById("qEditModal");
  if (!modal) return; // nothing to do on pages without the modal

  const dialog = modal.querySelector(".q-modal__dialog");
  const closeBtn = document.getElementById("qEditClose");
  const cancelBtn = document.getElementById("qEditCancel");
  const form = document.getElementById("qEditForm");
  const answersWrap = document.getElementById("qeAnswersWrap");
  const addAnswerBtn = document.getElementById("qeAddAnswer");

  // track which edit button opened the modal so we can update its dataset after AJAX saves
  let currentEditBtn = null;

  const elQId = document.getElementById("qeQuestionId");
  const elQSet = document.getElementById("qeQuestionSetId");
  const elQText = document.getElementById("qeQuestionText");
  const elQType = document.getElementById("qeQuestionType");
  const elQIsScored = document.getElementById("qeIsScored");
  const elQMaxPoints = document.getElementById("qeMaxPoints");
  const elQMaxPointsWrap = document.getElementById("qeMaxPointsWrap");

  function openModal() {
    modal.classList.add("open");
    document.body.style.overflow = "hidden";
  }

  function closeModal() {
    modal.classList.remove("open");
    document.body.style.overflow = "";
  }

  if (closeBtn) closeBtn.addEventListener("click", closeModal);
  if (cancelBtn) cancelBtn.addEventListener("click", closeModal);
  if (dialog)
    modal.addEventListener("click", (e) => {
      if (!dialog.contains(e.target)) closeModal();
    });

  function buildAnswerRow(idx, item, isSetScored = true) {
    const row = document.createElement("div");
    row.className = "q-ans-row";

    const colLabel = document.createElement("div");
    colLabel.className = "q-ans-col q-ans-col--label";
    const inLabel = document.createElement("input");
    inLabel.className = "q-input";
    inLabel.type = "text";
    inLabel.name = `Answers[${idx}].Label`;
    inLabel.maxLength = 5;
    inLabel.placeholder = "A, B, C...";
    inLabel.value = item?.label ?? "";
    colLabel.appendChild(inLabel);

    const colText = document.createElement("div");
    colText.className = "q-ans-col q-ans-col--text";
    const inText = document.createElement("input");
    inText.className = "q-input";
    inText.type = "text";
    inText.name = `Answers[${idx}].Text`;
    inText.placeholder = "Nội dung đáp án";
    inText.required = true;
    inText.value = item?.text ?? "";
    colText.appendChild(inText);

    const colHint = document.createElement("div");
    colHint.className = "q-ans-col q-ans-col--hint";
    const inHint = document.createElement("input");
    inHint.className = "q-input";
    inHint.type = "text";
    inHint.name = `Answers[${idx}].Hint`;
    inHint.placeholder = "Gợi ý (tuỳ chọn)";
    inHint.value = item?.hint ?? "";
    colHint.appendChild(inHint);

    // scoring controls: checkbox (award points)
    const colScore = document.createElement("div");
    colScore.className = "q-ans-col q-ans-col--score";

    // Only show scoring controls if the question set is scored
    if (isSetScored) {
      const labelChk = document.createElement("label");
      labelChk.style.display = "block";
      labelChk.style.fontSize = "0.9em";
      const inIsExclusive = document.createElement("input");
      inIsExclusive.type = "checkbox";
      inIsExclusive.name = `Answers[${idx}].IsExclusive`;
      inIsExclusive.value = "true";
      // check if backend indicates exclusive or has points > 0
      const hasPoints =
        item?.points !== undefined &&
        item?.points !== null &&
        Number(item.points) > 0;
      inIsExclusive.checked = !!item?.isExclusive || hasPoints;

      // Debug logging
      console.log(`Answer ${idx}:`, {
        points: item?.points,
        isExclusive: item?.isExclusive,
        hasPoints: hasPoints,
        checked: inIsExclusive.checked,
      });
      inIsExclusive.style.marginRight = "6px";
      labelChk.appendChild(inIsExclusive);
      labelChk.appendChild(document.createTextNode("Ghi điểm"));
      colScore.appendChild(labelChk);

      // Add hidden input to ensure false is sent when checkbox is unchecked
      const hidIsExclusive = document.createElement("input");
      hidIsExclusive.type = "hidden";
      hidIsExclusive.name = `Answers[${idx}].IsExclusive`;
      hidIsExclusive.value = "false";
      colScore.appendChild(hidIsExclusive);

      // Update hidden input when checkbox changes and persist via AJAX
      inIsExclusive.addEventListener("change", async function () {
        hidIsExclusive.disabled = this.checked;

        try {
          const qIdVal = elQId ? elQId.value : null;
          if (!qIdVal) return;
          const scoreRaw = elQMaxPoints ? elQMaxPoints.value : "0";

          // collect selected answer ids
          const rows = answersWrap.querySelectorAll('.q-ans-row');
          const selected = [];
          rows.forEach(r => {
            const chk = r.querySelector('input[type="checkbox"][name$=".IsExclusive"]');
            const hid = r.querySelector('input[type="hidden"][name$=".Id"]');
            if (chk && chk.checked && hid && hid.value) selected.push(parseInt(hid.value, 10));
          });

          const tokenEl = form.querySelector('input[name="__RequestVerificationToken"]');
          const token = tokenEl ? tokenEl.value : null;

          const fd = new FormData();
          fd.append('questionId', qIdVal);
          fd.append('score', scoreRaw || '0');
          selected.forEach(v => fd.append('selectedOptionIds', v));
          if (elQSet && elQSet.value) fd.append('redirectSetId', elQSet.value);
          fd.append('scoreTab', elQIsScored && elQIsScored.checked ? 'true' : 'false');
          if (token) fd.append('__RequestVerificationToken', token);

          const resp = await fetch('/Home/set-points', {
            method: 'POST',
            headers: { 'X-Requested-With': 'XMLHttpRequest' },
            body: fd,
            credentials: 'same-origin'
          });

          if (!resp.ok) {
            console.warn('Save points failed', resp.status);
            return;
          }

          let json = null;
          try { json = await resp.json(); } catch (e) { /* not JSON */ }

          if (json && json.ok && Array.isArray(json.answers)) {
            if (currentEditBtn) currentEditBtn.dataset.answers = JSON.stringify(json.answers);

            rows.forEach(r => {
              const hid = r.querySelector('input[type="hidden"][name$=".Id"]');
              const chk = r.querySelector('input[type="checkbox"][name$=".IsExclusive"]');
              if (!hid || !chk) return;
              const aid = parseInt(hid.value, 10);
              const serverAns = json.answers.find(x => x.id === aid);
              if (serverAns) {
                const should = (serverAns.isExclusive === true) || (serverAns.points && Number(serverAns.points) > 0);
                chk.checked = !!should;
                hid.disabled = chk.checked;
              }
            });
          }
        } catch (err) {
          console.error('Error saving points', err);
        }
      });
    } else {
      // For non-scored sets, add a placeholder or message
      colScore.innerHTML =
        '<span style="color: #999; font-size: 0.9em;">Không tính điểm</span>';
    }

    if (item?.id !== undefined && item?.id !== null) {
      const hid = document.createElement("input");
      hid.type = "hidden";
      hid.name = `Answers[${idx}].Id`;
      hid.value = item.id;
      row.appendChild(hid);
    }

    const colAct = document.createElement("div");
    colAct.className = "q-ans-col q-ans-col--act";
    const delBtn = document.createElement("button");
    delBtn.type = "button";
    delBtn.className = "q-btn q-btn--ghost q-btn--sm";
    delBtn.textContent = "Xóa";
    delBtn.addEventListener("click", () => {
      row.remove();
      renumberAnswerNames();
    });
    colAct.appendChild(delBtn);

    row.append(colLabel, colText, colHint, colScore, colAct);
    return row;
  }

  function renumberAnswerNames() {
    if (!answersWrap) return;
    const rows = answersWrap.querySelectorAll(".q-ans-row");
    rows.forEach((row, idx) => {
      row.querySelectorAll("input[name]").forEach((inp) => {
        inp.name = inp.name.replace(/Answers\[\d+\]\./, `Answers[${idx}].`);
      });
    });
  }

  if (addAnswerBtn && answersWrap) {
    addAnswerBtn.addEventListener("click", () => {
      const count = answersWrap.querySelectorAll(".q-ans-row").length;
      const isSetScored = elQIsScored ? elQIsScored.checked : true;
      const row = buildAnswerRow(
        count,
        { label: "", text: "", hint: "" },
        isSetScored
      );
      answersWrap.appendChild(row);
    });
  }

  document.querySelectorAll(".btn-edit-q").forEach((btn) => {
    btn.addEventListener("click", () => {
      currentEditBtn = btn;
      const qId = btn.dataset.qid;
      const qSet = btn.dataset.qset;
      const qType = btn.dataset.qtype;
      const qText = btn.dataset.qtext;
      let answers = [];
      try {
        answers = JSON.parse(btn.dataset.answers || "[]");
      } catch {}

      if (elQId) elQId.value = qId;
      if (elQSet) elQSet.value = qSet;
      if (elQType) elQType.value = qType || "single";
      if (elQText) elQText.value = qText || "";

      if (answersWrap) {
        answersWrap.querySelectorAll(".q-ans-row").forEach((x) => x.remove());
        const isSetScored = btn.dataset.issetscored === "1";
        answers.forEach((a, i) =>
          answersWrap.appendChild(buildAnswerRow(i, a, isSetScored))
        );
      }

      // populate scored / maxPoints
      if (elQIsScored) elQIsScored.checked = btn.dataset.issetscored === "1";
      if (elQMaxPoints) elQMaxPoints.value = btn.dataset.maxpoints || "";
      toggleMaxPointsVisibility();

      if (typeof toggleAnswersByType === "function") toggleAnswersByType();
      openModal();
    });
  });

  function toggleAnswersByType() {
    if (!elQType || !answersWrap) return;
    const type = elQType.value;
    answersWrap.style.display = type === "text" ? "none" : "";
    toggleMaxPointsVisibility();
  }

  function toggleMaxPointsVisibility() {
    if (!elQIsScored || !elQMaxPointsWrap) return;
    // show max points only when checkbox is checked and type is not text
    const show = elQIsScored.checked && (!elQType || elQType.value !== "text");
    elQMaxPointsWrap.style.display = show ? "" : "none";
  }

  if (elQIsScored)
    elQIsScored.addEventListener("change", toggleMaxPointsVisibility);

  if (elQType) elQType.addEventListener("change", toggleAnswersByType);
})();
